using UnityEngine;
using UnityEngine.Playables;
using Metroma.FocusCam;
using System.Collections.Generic;
using UnityEngine.Timeline;
using Metroma.CameraTool.Modules;
using NaughtyAttributes;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Metroma.CameraTool
{
    /// <summary>
    /// The CameraRig is the core controller that manages all modular camera behaviors.
    /// It coordinates the evaluation of the FocusCam system, transitions, and additive modules.
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-10)]
    [ExecuteAlways]
    public class CameraRig : MonoBehaviour
    {
        #region --- Service Access ---

        public static CameraRig Active { get; private set; }

        public event System.Action<CameraState> OnStateChanged;
        public event System.Action<CameraPose> OnPoseEventReached;
        public event System.Action<Timeline.CameraMarkerBase> OnMarkerEventHit;

        #endregion

        #region --- Serialized Fields ---

        [Header("Hardware")]
        [Required("Target Camera is necessary to apply calculated poses.")]
        [SerializeField] private UnityEngine.Camera targetCamera;

        [Header("Modules")]
        [Tooltip("If checked, the rig will automatically control the target camera transform.")]
        [SerializeField] private bool autoHandleTransform = true;

        [Header("Debug")]
        public bool showSceneGizmos = true;
        public bool showRuntimeHUD = false;

        // --- Editor Persistence ---
#pragma warning disable 0414
        [SerializeField, HideInInspector] private bool foldReferences = true;
        [SerializeField, HideInInspector] private bool foldAnimation = true;
        [SerializeField, HideInInspector] private bool foldEvents = false;
        [SerializeField, HideInInspector] private bool foldViewport = false;
        [SerializeField, HideInInspector] private bool foldHaptics = false;
        [SerializeField, HideInInspector] private bool foldDebug = false;
#pragma warning restore 0414

        #endregion

        #region --- Runtime State ---

        private Transform _cameraTransform;
        private List<ICameraModule> _modules = new List<ICameraModule>();
        
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
        
        /// <summary> Returns true if the rig is currently being controlled by a Timeline Mixer. </summary>
        public bool IsDrivenByTimeline => _isDrivenByTimeline;

        public TransitionModule Transitions { get { if (!_isInitialized) InitializeRig(); return _transitionModule; } }
        public SequenceModule Sequences { get { if (!_isInitialized) InitializeRig(); return _sequenceModule; } }
        public FPSModule FPS { get { if (!_isInitialized) InitializeRig(); return _fpsModule; } }

        public PlayableDirector EditorDirector => Sequences != null ? Sequences.Director : null;

        #endregion

        #region --- Lifecycle ---

        private void Awake()
        {
            InitializeRig();
            
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
            if (!Application.isPlaying || !autoHandleTransform)
                return;

            if (!_isInitialized)
                InitializeRig();

            float deltaTime = Time.deltaTime;
            Internal_ManualUpdate(deltaTime);
        }

        /// <summary> DRIVER: Manually force a module update and pose application. Useful for coroutines. </summary>
        public void Internal_ManualUpdate(float InDeltaTime)
        {
            if (!_isInitialized)
                InitializeRig();

            UpdateModules(InDeltaTime);
            ApplyFinalPose();
        }

        #endregion

        #region --- Internal Logic ---

        private void InitializeRig()
        {
            if (targetCamera == null)
            {
                targetCamera = UnityEngine.Camera.main;
                
                if (targetCamera == null)
                {
                    targetCamera = GetComponentInChildren<UnityEngine.Camera>(true);
                }
            }

            if (targetCamera != null)
            {
                _cameraTransform = targetCamera.transform;
                
                // Capture initial pose
                _currentPose = new CameraPose
                {
                    position = _cameraTransform.position,
                    rotation = _cameraTransform.rotation,
                    fov = targetCamera.fieldOfView,
                    up = _cameraTransform.up
                };
            }
            
            _transitionModule = GetOrAddComponent<TransitionModule>();
            _sequenceModule = GetOrAddComponent<SequenceModule>();
            _fpsModule = GetOrAddComponent<FPSModule>();

            _modules.Clear();
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
            // 0. STARTING POINT: Use current pose as base, but ensure it's not stale 
            // If nothing has driven the camera yet, start from the physical transform
            if (!_isDrivenByTimeline && (!Transitions || !Transitions.IsActive))
            {
                _currentPose.position = _cameraTransform.position;
                _currentPose.rotation = _cameraTransform.rotation;
                _currentPose.fov = targetCamera ? targetCamera.fieldOfView : 60f;
            }

            // 1. BASE LAYER: Timeline input (Overrides physical transform if active)
            if (_isDrivenByTimeline)
            {
                _currentPose = _timelinePose;
                _isDrivenByTimeline = false;
            }

            // 2. MODIFIER LAYER: Transitions & Modules (Layered application)
            // If FPS is active, it usually wants to bypass the current transition state
            bool skipTransition = _fpsModule && _fpsModule.IsActive;

            if (Transitions && Transitions.IsActive && !skipTransition)
            {
                _currentPose = Transitions.ModifyPose(_currentPose);
            }

            if (_fpsModule && _fpsModule.IsActive)
            {
                _currentPose = _fpsModule.ModifyPose(_currentPose);
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
        public void ApplyTimelineState(CameraPose InPose)
        {
            _timelinePose = InPose;
            _isDrivenByTimeline = true;
            _currentPose = InPose; // Update for debug gizmos

            if (!Application.isPlaying)
            {
                var cam = targetCamera != null ? targetCamera : GetComponentInChildren<UnityEngine.Camera>(true);
        
                if (cam != null)
                {
                    cam.transform.position = InPose.position;
                    cam.transform.rotation = InPose.rotation;
                    cam.fieldOfView = InPose.fov;
                    
#if UNITY_EDITOR
                    UnityEditor.EditorUtility.SetDirty(cam.transform);
                    UnityEditor.SceneView.RepaintAll();
#endif
                }
            }
        }

        public void Internal_NotifyMarkerHit(Timeline.CameraMarkerBase InMarker) => OnMarkerEventHit?.Invoke(InMarker);
        public void Internal_NotifyPoseReached(CameraPose InPose) => OnPoseEventReached?.Invoke(InPose);
        public void Internal_NotifyStateChanged(CameraState InState) => OnStateChanged?.Invoke(InState);

        public void TransitionToPose(CameraPose InTarget, float InDuration, AnimationCurve InCurve = null, DirectorAction InAction = DirectorAction.None)
        {
            _transitionModule.StartTransition(InTarget, InDuration, InCurve);
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
            return _isDrivenByTimeline ? _timelinePose : _currentPose;
        }

        #endregion
        
        #region --- Debug & Gizmos ---

        private void OnDrawGizmos()
        {
            if (!showSceneGizmos) return;

            // Ensure modules are available for gizmos
            if (_sequenceModule == null) _sequenceModule = GetComponent<SequenceModule>();

            // 1. Draw Target Pose Frustum
            if (targetCamera != null)
            {
                Quaternion rot = _currentPose.rotation;
                if (Mathf.Approximately(rot.x * rot.x + rot.y * rot.y + rot.z * rot.z + rot.w * rot.w, 0f))
                    rot = Quaternion.identity;

                Matrix4x4 temp = Gizmos.matrix;
                Gizmos.matrix = Matrix4x4.TRS(_currentPose.position, rot, Vector3.one);
                
                Gizmos.color = new Color(0.95f, 0.2f, 0.45f, 0.8f);
                Gizmos.DrawFrustum(Vector3.zero, _currentPose.fov, 3f, 0.1f, targetCamera.aspect);
                
                Gizmos.matrix = temp;
                Gizmos.color = new Color(0.95f, 0.2f, 0.45f, 0.3f);
                Gizmos.DrawRay(_currentPose.position, rot * Vector3.forward * 5f);
            }

            // 2. Timeline Storyboard Visualization
#if UNITY_EDITOR
            if (_sequenceModule != null && _sequenceModule.DebugFocusTimeline != null)
            {
                DrawTimelineGizmos(_sequenceModule.DebugFocusTimeline);
            }
#endif

            // 3. Draw Rig Pivot
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, 0.2f);
        }

#if UNITY_EDITOR
        private void DrawTimelineGizmos(UnityEngine.Timeline.TimelineAsset timeline)
        {
            foreach (var track in timeline.GetOutputTracks())
            {
                if (track is FocusCamTrack focusTrack)
                {
                    foreach (var clip in focusTrack.GetClips())
                    {
                        var focusClip = clip.asset as FocusCamClip;
                        if (focusClip == null) continue;

                        Color clipColor = focusClip.mode == FocusMode.LookAtPoint 
                            ? new Color(0.95f, 0.2f, 0.45f, 0.5f) 
                            : new Color(0.15f, 0.6f, 1.0f, 0.5f);
                        
                        if (focusClip.useCustomColor) clipColor = focusClip.customColor;
                        clipColor.a = 0.5f;

                        // Target Point
                        if (focusClip.mode == FocusMode.LookAtPoint)
                        {
                            Gizmos.color = clipColor;
                            Gizmos.DrawSphere(focusClip.position, 0.3f);
                            UnityEditor.Handles.Label(focusClip.position + Vector3.up * 0.4f, $"Clip: {clip.displayName}", UnityEditor.EditorStyles.miniLabel);
                        }

                        // Camera Placement
                        if (focusClip.overridePosition)
                        {
                            Gizmos.color = clipColor;
                            Gizmos.DrawWireCube(focusClip.cameraPosition, Vector3.one * 0.2f);
                            Gizmos.color = new Color(clipColor.r, clipColor.g, clipColor.b, 0.2f);
                            Gizmos.DrawLine(focusClip.cameraPosition, focusClip.mode == FocusMode.LookAtPoint ? focusClip.position : focusClip.cameraPosition + Quaternion.Euler(focusClip.rotation) * Vector3.forward * 2f);
                        }
                    }
                }
            }
        }
#endif

        private void OnGUI()
        {
            if (!showRuntimeHUD || !Application.isPlaying) return;

            Rect rect = new Rect(20, 20, 250, 150);
            GUI.Box(rect, "METROMA CAMERA DEBUG");

            using (new GUILayout.AreaScope(new Rect(rect.x + 10, rect.y + 25, rect.width - 20, rect.height - 30)))
            {
                GUILayout.Label($"<b>Active Instance:</b> {name}", GetDebugLabelStyle());
                GUILayout.Space(5);
                
                DrawDebugStat("Sequence", _sequenceModule != null && _sequenceModule.IsPlayingFocus ? "PLAYING" : "IDLE");
                DrawDebugStat("Transition", _transitionModule != null && _transitionModule.IsActive ? "ACTIVE" : "IDLE");
                DrawDebugStat("FPS", _fpsModule != null && _fpsModule.IsActive ? "ACTIVE" : "IDLE");
                
                GUILayout.Space(5);
                GUILayout.Label($"<b>Pose:</b> {_currentPose.position.x:F1}, {_currentPose.position.y:F1}, {_currentPose.position.z:F1}");
                GUILayout.Label($"<b>FOV:</b> {_currentPose.fov:F1}");
            }
        }

        private void DrawDebugStat(string label, string status)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label($"{label}:", GUILayout.Width(80));
            GUI.color = status == "IDLE" ? Color.gray : Color.cyan;
            GUILayout.Label(status);
            GUI.color = Color.white;
            GUILayout.EndHorizontal();
        }

        private GUIStyle GetDebugLabelStyle()
        {
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.richText = true;
            style.fontSize = 11;
            return style;
        }

        #endregion
    }
}
