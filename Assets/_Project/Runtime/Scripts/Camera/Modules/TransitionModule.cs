using UnityEngine;

namespace Metroma.CameraTool.Modules
{
    /// <summary>
    /// Module responsible for handling transitions between poses (LookAt, Snapping, Smoothing).
    /// </summary>
    public class TransitionModule : MonoBehaviour, ICameraModule
    {
        #region --- Runtime State ---

        private CameraRig _rig;
        private CameraPose _startPose;
        private CameraPose _targetPose;
        
        private float _transitionAlpha;
        private float _transitionDuration;
        private float _transitionTime;
        private AnimationCurve _transitionCurve;

        private float _rotationReturnTimer = 0f;
        private float _returnDuration = 0f;
        private CameraPose _returnStartPose;

        private bool _isTransitioning = false;
        private bool _isStaticPose = false;

        #endregion

        #region --- Properties ---

        public int Priority => 10;
        public bool IsActive => _isTransitioning || _isStaticPose || _rotationReturnTimer > 0;

        #endregion

        #region --- ICameraModule Implementation ---

        public void Initialize(CameraRig InRig)
        {
            _rig = InRig;
        }

        public void OnUpdate(float InDeltaTime)
        {
            UpdateTransitionState(InDeltaTime);
            UpdateRotationReturn(InDeltaTime);
        }

        #endregion

        #region --- Logic ---

        public CameraPose ModifyPose(CameraPose InBasePose)
        {
            CameraPose finalPose = InBasePose;

            if (_isTransitioning)
            {
                // Core transition blend
                finalPose = CameraPose.Lerp(_startPose, _targetPose, _transitionAlpha);
                
                // SMOOTH ARRIVAL: If we have an auto-return duration, start "leaking" the InBasePose
                // during the last 20% of the transition to make the handover invisible.
                if (_autoReturnDuration > 0.01f && _transitionAlpha > 0.8f)
                {
                    float arrivalAlpha = (_transitionAlpha - 0.8f) / 0.2f; // 0 to 1 over the last 20%
                    arrivalAlpha = Mathf.SmoothStep(0f, 1f, arrivalAlpha);
                    
                    // Blend from the static target towards the dynamic base (Timeline/Rail)
                    finalPose.position = Vector3.Lerp(finalPose.position, InBasePose.position, arrivalAlpha * 0.5f);
                    finalPose.rotation = Quaternion.Slerp(finalPose.rotation, InBasePose.rotation, arrivalAlpha * 0.5f);
                }

                if (_rig.Rails && _rig.Rails.LookAtTarget)
                {
                    Vector3 direction = _rig.Rails.LookAtTarget.position - finalPose.position;
                    if (direction.sqrMagnitude > 0.001f)
                    {
                        Vector3 currentUp = finalPose.up != Vector3.zero ? finalPose.up : Vector3.up;
                        Quaternion dynamicRot = Quaternion.LookRotation(direction, currentUp);
                
                        finalPose.rotation = Quaternion.Slerp(_startPose.rotation, dynamicRot, _transitionAlpha);
                    }
                }
                else
                {
                    finalPose.rotation = Quaternion.Slerp(_startPose.rotation, _targetPose.rotation, _transitionAlpha);
                }
            }
            else if (_isStaticPose)
            {
                finalPose = _targetPose;
            }

            if (!_isTransitioning && _rotationReturnTimer > 0)
            {
                float alpha = 1f - (_rotationReturnTimer / _returnDuration);
                
                alpha = Mathf.SmoothStep(0f, 1f, alpha);

                finalPose.position = Vector3.Lerp(_returnStartPose.position, finalPose.position, alpha);
                finalPose.rotation = Quaternion.Slerp(_returnStartPose.rotation, finalPose.rotation, alpha);
                finalPose.fov = Mathf.Lerp(_returnStartPose.fov, finalPose.fov, alpha);
            }

            return finalPose;
        }

        private float _autoReturnDuration = 0f;

        public void StartTransition(CameraPose InTargetPose, float InDuration, AnimationCurve InCurve = null, float InAutoReturnDuration = 0f)
        {
            if (!_rig)
                _rig = GetComponent<CameraRig>();
            
            if (!_rig)
                return;
            
            _startPose = new CameraPose 
            {
                position = _rig.CameraTransform.position,
                rotation = _rig.CameraTransform.rotation,
                fov = _rig.TargetCamera ? _rig.TargetCamera.fieldOfView : 60f,
                up = _rig.CameraTransform.up
            };

            _targetPose = InTargetPose;
            _transitionDuration = InDuration;
            _transitionTime = 0f;
            _transitionCurve = InCurve ?? AnimationCurve.EaseInOut(0, 0, 1, 1);
            _transitionAlpha = 0f;
            _autoReturnDuration = InAutoReturnDuration;
            
            _isTransitioning = true;
            _isStaticPose = false;

            _rig.Internal_NotifyStateChanged(CameraState.Transitioning);
        }

        public void SnapToPose(CameraPose InPose)
        {
            _targetPose = InPose;
            _isStaticPose = true;
            _isTransitioning = false;
            _rotationReturnTimer = 0f;
            _autoReturnDuration = 0f;

            _rig.Internal_NotifyStateChanged(CameraState.StaticPose);
            _rig.Internal_NotifyPoseReached(InPose);
        }

        public void ReturnToRail(float InSmoothness = 5.0f)
        {
            _isStaticPose = false;
            _isTransitioning = false;
            
            if (InSmoothness > 100f || InSmoothness <= 0.01f)
            {
                _rotationReturnTimer = 0f;
                _returnDuration = 0f;
                _rig.Internal_NotifyStateChanged(CameraState.ReturningToRail);
                
                return;
            }
            
            // Treat InSmoothness as a speed coefficient (High = Fast, Low = Slow)
            _returnDuration = Mathf.Max(0.001f, 1.0f / InSmoothness);
            _rotationReturnTimer = _returnDuration; 

            if (_rig && _rig.CameraTransform)
            {
                _returnStartPose = new CameraPose 
                {
                    position = _rig.CameraTransform.position,
                    rotation = _rig.CameraTransform.rotation,
                    fov = _rig.TargetCamera ? _rig.TargetCamera.fieldOfView : 60f,
                    up = _rig.CameraTransform.up
                };
            }

            _rig.Internal_NotifyStateChanged(CameraState.ReturningToRail);
        }

        private void UpdateTransitionState(float InDeltaTime)
        {
            if (!_isTransitioning)
                return;

            _transitionTime += InDeltaTime;
            float t = Mathf.Clamp01(_transitionTime / _transitionDuration);
            _transitionAlpha = _transitionCurve.Evaluate(t);

            if (t >= 1f)
            {
                _isTransitioning = false;

                if (_autoReturnDuration > 0.01f)
                {
                    ReturnToRail(_autoReturnDuration);
                }
                else
                {
                    _isStaticPose = true;
                    _rig.Internal_NotifyStateChanged(CameraState.StaticPose);
                    _rig.Internal_NotifyPoseReached(_targetPose);
                }
            }
        }

        private void UpdateRotationReturn(float InDeltaTime)
        {
            if (_rotationReturnTimer > 0)
            {
                _rotationReturnTimer -= InDeltaTime;
                
                if (_rotationReturnTimer < 0)
                    _rotationReturnTimer = 0;
            }
        }

        #endregion
    }
}
