using UnityEngine;
using UnityEngine.Playables;
using System.Collections.Generic;
using Metroma.CameraTool.Modules;
using NaughtyAttributes;

namespace Metroma.CameraTool
{
    /// <summary>
    /// The CameraRig is the core controller that manages all modular camera behaviors.
    /// It coordinates the evaluation of splines, transitions, and timeline sequences.
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-10)]
    public class CameraRig : MonoBehaviour
    {
        #region --- Service Access ---

        public static CameraRig Active { get; private set; }

        public event System.Action<CameraState> OnStateChanged;
        public event System.Action<CameraPose> OnPoseEventReached;
        public event System.Action<CameraChapter> OnChapterStarted;
        public event System.Action<Timeline.CameraMarkerBase> OnMarkerEventHit;
        public UnityEngine.Events.UnityEvent<string> onSplineNotified;

        #endregion

        #region --- Serialized Fields ---

        [Header("Hardware")]
        [Required("Target Camera is necessary to apply calculated poses.")]
        [SerializeField] private UnityEngine.Camera targetCamera;

        [Header("Modules")]
        [Tooltip("If checked, the rig will automatically control the target camera transform.")]
        [SerializeField] private bool autoHandleTransform = true;

        // --- Editor Persistence ---
#pragma warning disable 0414
        [SerializeField, HideInInspector] private bool foldReferences = true;
        [SerializeField, HideInInspector] private bool foldChapters = true;
        [SerializeField, HideInInspector] private bool foldSegments = true;
        [SerializeField, HideInInspector] private bool foldAnimation = true;
        [SerializeField, HideInInspector] private bool foldEvents = false;
        [SerializeField, HideInInspector] private bool foldViewport = false;
        [SerializeField, HideInInspector] private bool foldHaptics = false;
        [SerializeField, HideInInspector] private bool foldDebug = false;
        [SerializeField, HideInInspector] private int selectedChapterIndex = 0;
#pragma warning restore 0414

        #endregion

        #region --- Runtime State ---

        private Transform _cameraTransform;
        private List<ICameraModule> _modules = new List<ICameraModule>();
        
        private RailModule _railModule;
        private TransitionModule _transitionModule;
        private SequenceModule _sequenceModule;
        private FPSModule _fpsModule;

        private CameraPose _currentPose;
        private bool _isInitialized = false;

        #endregion

        #region --- Public Properties ---

        public UnityEngine.Camera TargetCamera => targetCamera;
        public Transform CameraTransform
        {
            get
            {
                if (_cameraTransform == null)
                {
                    if (targetCamera == null) targetCamera = GetComponentInChildren<UnityEngine.Camera>(true);
                    if (targetCamera != null) _cameraTransform = targetCamera.transform;
                }
                return _cameraTransform;
            }
        }
        public CameraPose CurrentPose => _currentPose;
        public bool IsControlActive => autoHandleTransform;

        public RailModule Rails => _railModule != null ? _railModule : (_railModule = GetComponent<RailModule>());
        public TransitionModule Transitions => _transitionModule != null ? _transitionModule : (_transitionModule = GetComponent<TransitionModule>());
        public SequenceModule Sequences => _sequenceModule != null ? _sequenceModule : (_sequenceModule = GetComponent<SequenceModule>());
        public FPSModule FPS => _fpsModule != null ? _fpsModule : (_fpsModule = GetComponent<FPSModule>());

        public Transform CurrentLookAtTarget => Rails != null ? Rails.LookAtTarget : null;
        public PlayableDirector EditorDirector => Sequences != null ? Sequences.Director : null;

        #endregion

        #region --- Lifecycle ---

        private void Awake()
        {
            if (Active != null && Active != this)
            {
                Debug.LogWarning("[CameraRig] Multiple instances detected. Overwriting active instance.");
            }

            Active = this;
            InitializeRig();
        }

        private void OnDestroy()
        {
            if (Active == this)
            {
                Active = null;
            }
        }

        private void LateUpdate()
        {
            if (!_isInitialized || !autoHandleTransform)
            {
                return;
            }

            float deltaTime = Time.deltaTime;
            UpdateModules(deltaTime);
            ApplyFinalPose();
        }

        #endregion

        #region --- Internal Logic ---

        private void InitializeRig()
        {
            if (targetCamera == null)
            {
                targetCamera = UnityEngine.Camera.main;
                
                // Editor Fallback: Camera.main might be null in the scene view
                if (targetCamera == null)
                {
                    targetCamera = GetComponentInChildren<UnityEngine.Camera>(true);
                }
            }

            if (targetCamera != null)
            {
                _cameraTransform = targetCamera.transform;
            }
            
            _railModule = GetOrAddComponent<RailModule>();
            _transitionModule = GetOrAddComponent<TransitionModule>();
            _sequenceModule = GetOrAddComponent<SequenceModule>();
            _fpsModule = GetOrAddComponent<FPSModule>();

            _modules.Clear();
            _modules.Add(_railModule);
            _modules.Add(_transitionModule);
            _modules.Add(_sequenceModule);
            _modules.Add(_fpsModule);

            foreach (var module in _modules)
            {
                module.Initialize(this);
            }

            _isInitialized = targetCamera != null;
        }

        private void UpdateModules(float InDeltaTime)
        {
            foreach (var module in _modules)
            {
                if (module.IsActive)
                {
                    module.OnUpdate(InDeltaTime);
                }
            }
        }

        private void ApplyFinalPose()
        {
            if (_isDrivenByTimeline)
            {
                _currentPose = _timelinePose;
                _isDrivenByTimeline = false;
            }
            else
            {
                if (Rails)
                {
                    _currentPose = Rails.CalculateTargetPose();
                }
            }

            // FPS Module Override (Priority 20 — highest)
            if (_fpsModule && _fpsModule.IsActive)
            {
                _currentPose = _fpsModule.ModifyPose(_currentPose);
            }
            else if (Transitions && Transitions.IsActive)
            {
                _currentPose = Transitions.ModifyPose(_currentPose);
            }

            if (CameraTransform)
            {
                CameraTransform.SetPositionAndRotation(_currentPose.position, _currentPose.rotation);
                if (targetCamera)
                    targetCamera.fieldOfView = _currentPose.fov;
            }
        }

        private CameraPose _timelinePose;
        private bool _isDrivenByTimeline = false;

        /// <summary> Called by Timeline Mixer to apply calculated state. </summary>
        public void ApplyTimelineState(CameraPose InPose, float InLookAtWeight)
        {
            _timelinePose = InPose;
            if (Rails != null)
                Rails.LookAtWeight = InLookAtWeight;
            
            _isDrivenByTimeline = true;

            if (!Application.isPlaying)
            {
                var cam = targetCamera != null ? targetCamera : GetComponentInChildren<UnityEngine.Camera>(true);
        
                if (cam != null)
                {
                    cam.transform.position = InPose.position;
                    cam.transform.rotation = InPose.rotation;
                    cam.fieldOfView = InPose.fov;
                    
                    UnityEditor.EditorUtility.SetDirty(cam.transform);
                    UnityEditor.SceneView.RepaintAll();
                }
                else
                {
                    Debug.LogError($"[CameraTool] CRITICAL: No Camera found on {gameObject.name}!");
                }
            }
        }

        public void Internal_NotifyChapterStarted(CameraChapter InChapter) => OnChapterStarted?.Invoke(InChapter);
        public void Internal_NotifyMarkerHit(Timeline.CameraMarkerBase InMarker) => OnMarkerEventHit?.Invoke(InMarker);
        public void Internal_NotifyPoseReached(CameraPose InPose) => OnPoseEventReached?.Invoke(InPose);
        public void Internal_NotifyStateChanged(CameraState InState) => OnStateChanged?.Invoke(InState);

        public CameraPose GetPoseOnRail(int InRailIdx, float InLocalT, int InChapterIdx) => Rails != null ? Rails.SampleRailLocal(InRailIdx, InLocalT) : CameraPose.Identity;

        public void TransitionToPose(CameraPose InTarget, float InDuration, AnimationCurve InCurve = null, DirectorAction InAction = DirectorAction.None)
        {
            _transitionModule.StartTransition(InTarget, InDuration, InCurve);
        }
        
        // --- Editor Bridge ---
        public void EditorReportVisualState(int InChapterIdx, int InRailIdx, float InProgress)
        {
            if (!_isInitialized)
            {
                InitializeRig();
            }

            if (InRailIdx >= 0)
            {
                float globalProgress = (InRailIdx + InProgress) / Mathf.Max(1, Rails.RailCount);
                Rails.GlobalProgress = globalProgress;
            }
            else
            {
                Rails.GlobalProgress = InProgress;
            }
            
            ApplyFinalPose();
        }

        private T GetOrAddComponent<T>() where T : Component
        {
            T component = GetComponent<T>();
            if (component == null)
            {
                component = gameObject.AddComponent<T>();
            }
            
            return component;
        }

        #endregion

        #region --- Public API ---

        public void SetControlActive(bool InActive)
        {
            autoHandleTransform = InActive;
        }

        public void SnapToTransform(Transform InTarget, bool InMatchFOV = true)
        {
            if (!InTarget || !targetCamera)
                return;

            targetCamera.transform.SetPositionAndRotation(InTarget.position, InTarget.rotation);
            if (InMatchFOV && InTarget.TryGetComponent<UnityEngine.Camera>(out var otherCam))
                targetCamera.fieldOfView = otherCam.fieldOfView;

            CameraPose snapPose = new CameraPose
            {
                position = InTarget.position,
                rotation = InTarget.rotation,
                fov = targetCamera.fieldOfView,
                up = InTarget.up
            };
            
            _currentPose = snapPose;
            _transitionModule.SnapToPose(snapPose);
        }
        
        public CameraPose GetTrueTargetPose()
        {
            CameraPose pose = _isDrivenByTimeline ? _timelinePose : Rails.CalculateTargetPose();
            
            if (Rails && Rails.LookAtTarget)
            {
                Vector3 direction = Rails.LookAtTarget.position - pose.position;
                if (direction.sqrMagnitude > 0.001f)
                {
                    pose.rotation = Quaternion.LookRotation(direction, pose.up);
                }
            }
            return pose;
        }

        #endregion
    }
}
