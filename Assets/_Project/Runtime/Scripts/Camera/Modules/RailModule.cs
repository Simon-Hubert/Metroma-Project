using UnityEngine;
using System.Collections.Generic;
using Dreamteck.Splines;
using NaughtyAttributes;

namespace Metroma.CameraTool.Modules
{
    /// <summary>
    /// Module responsible for calculating the camera pose based on spline rails.
    /// Handles Pacing (Segmented, Global, Constant) and evaluation logic.
    /// </summary>
    public class RailModule : MonoBehaviour, ICameraModule
    {
        #region --- Serialized Fields ---

        [SerializeField] private List<SplineComputer> splineRails = new List<SplineComputer>();
        
        [Range(0f, 1f)]
        [SerializeField] private float globalProgress;

        [Tooltip("Blend between spline rotation (0) and LookAt rotation (1).")]
        [Range(0f, 1f)]
        [SerializeField] private float lookAtWeight = 1f;

        [Tooltip("Optional: default target to look at.")]
        [SerializeField] private Transform lookAtTarget;

        [Tooltip("Optional: list of potential targets to look at. Switch via markers.")]
        [SerializeField] private List<Transform> lookAtTargets = new List<Transform>();

        [SerializeField] private float defaultFOV = 60f;

        [Tooltip("Physical distance (in meters) to blend between two rails in a chapter.")]
        [SerializeField] private float junctionBlendDistance = 0.5f;

        [Tooltip("Visual smoothness of the rail-to-rail transition.")]
        [SerializeField] private float junctionSmoothness = 5.0f;

        #endregion

        #region --- Runtime State ---

        private CameraRig _rig;
        private SplineComputer[] _cachedRails;
        
        private Transform _lookAtTarget;
        private Transform _lookAtFrom;
        private float _lookAtLerp;
        private float _lookAtDuration;

        private bool _isInitialized = false;

        #endregion

        #region --- Properties ---

        public int Priority => 0;
        public bool IsActive => _isInitialized;
        
        public float GlobalProgress { get => globalProgress; set => globalProgress = Mathf.Clamp01(value); }
        public float LookAtWeight { get => lookAtWeight; set => lookAtWeight = Mathf.Clamp01(value); }
        public float JunctionBlendDistance { get => junctionBlendDistance; set => junctionBlendDistance = Mathf.Max(0f, value); }
        public float JunctionSmoothness { get => junctionSmoothness; set => junctionSmoothness = Mathf.Clamp(value, 1f, 20f); }
        public Transform LookAtTarget => _lookAtTarget;

        #endregion

        #region --- ICameraModule Implementation ---

        public void Initialize(CameraRig InRig)
        {
            _rig = InRig;
            RefreshRailCache();
            _isInitialized = _cachedRails != null && _cachedRails.Length > 0;
        }

        public void OnUpdate(float InDeltaTime)
        {
            UpdateLookAtTimers(InDeltaTime);
        }

        #endregion

        #region --- Logic ---

        public void RefreshRailCache()
        {
            List<SplineComputer> validRails = new List<SplineComputer>();
            foreach (var rail in splineRails)
            {
                if (rail)
                {
                    validRails.Add(rail);
                }
            }

            _cachedRails = validRails.ToArray();
        }

        public CameraPose CalculateTargetPose()
        {
            if (!_isInitialized)
            {
                RefreshRailCache();
                if (_cachedRails == null || _cachedRails.Length == 0)
                    return CameraPose.Identity;
            }
            
            CameraPose pose = EvaluateAt(globalProgress);
            pose = ApplyLookAt(pose);

            return pose;
        }

        private CameraPose EvaluateAt(float InProgress)
        {
            if (_cachedRails == null || _cachedRails.Length == 0)
            {
                return CameraPose.Identity;
            }
            
            int railCount = _cachedRails.Length;
            float railStep = 1f / railCount;
            int railIndex = Mathf.Clamp((int)(InProgress / railStep), 0, railCount - 1);
            float localT = (InProgress - (railIndex * railStep)) / railStep;

            CameraPose pose = SampleRailLocal(railIndex, localT);

            // JUNCTION BLENDING (Distance-based for non-timeline use)
            if (junctionBlendDistance > 0.01f && railCount > 1)
            {
                float railLength = _cachedRails[railIndex].CalculateLength();
                float normalizedBlendRange = (junctionBlendDistance * 0.5f) / Mathf.Max(0.01f, railLength);
                
                // Case A: Near end of current rail
                if (localT > (1f - normalizedBlendRange) && railIndex < railCount - 1)
                {
                    float localAlpha = (localT - (1f - normalizedBlendRange)) / normalizedBlendRange;
                    float blendAlpha = Mathf.SmoothStep(0f, 1f, localAlpha * 0.5f);
                    
                    if (junctionSmoothness > 1.1f)
                        blendAlpha = Mathf.Pow(blendAlpha * 2f, junctionSmoothness) * 0.5f;

                    CameraPose nextPose = SampleRailLocal(railIndex + 1, 0f);
                    return CameraPose.Lerp(pose, nextPose, blendAlpha);
                }

                // Case B: Near start of current rail
                if (localT < normalizedBlendRange && railIndex > 0)
                {
                    float localAlpha = localT / normalizedBlendRange;
                    float blendAlpha = Mathf.SmoothStep(0f, 1f, 0.5f + (localAlpha * 0.5f));
                    
                    if (junctionSmoothness > 1.1f)
                        blendAlpha = 1f - (Mathf.Pow((1f - blendAlpha) * 2f, junctionSmoothness) * 0.5f);

                    CameraPose prevPose = SampleRailLocal(railIndex - 1, 1f);
                    return CameraPose.Lerp(prevPose, pose, blendAlpha);
                }
            }

            return pose;
        }

        public CameraPose SampleRailLocal(int InRailIdx, float InLocalT)
        {
            if (_cachedRails == null || _cachedRails.Length == 0)
                RefreshRailCache();
            
            if (_cachedRails == null || _cachedRails.Length == 0)
            {
                return CameraPose.Identity;
            }

            if (InRailIdx < 0 || InRailIdx >= _cachedRails.Length || !_cachedRails[InRailIdx])
            {
                return CameraPose.Identity;
            }

            if (!Application.isPlaying)
                _cachedRails[InRailIdx].Rebuild();

            SplineSample sample = _cachedRails[InRailIdx].Evaluate(InLocalT);
            CameraPose pose = CameraPose.Identity;
            pose.position = sample.position;
            pose.rotation = sample.rotation;
            pose.fov = defaultFOV;
            pose.up = sample.up;
            pose.railIdx = InRailIdx;
            pose.progressInsideRail = InLocalT;
            
            return pose;
        }

        private CameraPose ApplyLookAt(CameraPose InPose)
        {
            if (!_lookAtTarget || lookAtWeight <= 0.001f)
            {
                return InPose;
            }

            Vector3 targetPos = _lookAtTarget.position;
            
            if (_lookAtLerp < 1f && _lookAtFrom)
            {
                targetPos = Vector3.Lerp(_lookAtFrom.position, _lookAtTarget.position, Mathf.SmoothStep(0, 1, _lookAtLerp));
            }

            Vector3 direction = targetPos - InPose.position;
            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion lookRot = Quaternion.LookRotation(direction, InPose.up);
                InPose.rotation = Quaternion.Slerp(InPose.rotation, lookRot, lookAtWeight);
            }

            return InPose;
        }

        private void UpdateLookAtTimers(float InDeltaTime)
        {
            if (_lookAtLerp < 1f && _lookAtDuration > 0)
            {
                _lookAtLerp += InDeltaTime / _lookAtDuration;
            }
        }

        public void SetLookAt(Transform InTarget, float InDuration = 1f)
        {
            Transform actualTarget = InTarget != null ? InTarget : lookAtTarget;
            if (_lookAtTarget == actualTarget)
                return;

            _lookAtFrom = _lookAtTarget;
            _lookAtTarget = actualTarget;
            _lookAtDuration = InDuration;
            _lookAtLerp = InDuration > 0 ? 0f : 1f;
        }

        public void HandleLookAtSwitchByIndex(int InIndex, float InDuration)
        {
            Transform target = (InIndex >= 0 && InIndex < lookAtTargets.Count) ? lookAtTargets[InIndex] : null;
            SetLookAt(target, InDuration);
        }

        public void SwitchToRail(int InIdx)
        {
            if (_cachedRails != null && InIdx >= 0 && InIdx < _cachedRails.Length)
            {
                globalProgress = (float)InIdx / _cachedRails.Length;
            }
        }

        public void ResetToChainMode()
        {
            globalProgress = 0f;
        }

        #endregion

        #region --- Editor Access ---

        public List<SplineComputer> EditorRails => splineRails;
        public int RailCount => (splineRails != null) ? splineRails.Count : 0;

        #endregion
    }
}
