using UnityEngine;
using UnityEngine.InputSystem;
using Metroma.CameraTool.Modifiers;

namespace Metroma.CameraTool.Modules
{
    /// <summary>
    /// Module that freezes camera position and gives input-driven Pitch/Yaw control.
    /// Driven by a <see cref="CameraFPSProfile"/> for all parameters.
    /// Triggered by <see cref="Timeline.CameraFPSMarker"/> via the Timeline.
    /// Priority 20 — highest in the module chain, overrides Transition (10).
    /// </summary>
    public class FPSModule : MonoBehaviour, ICameraModule
    {
        // --- Runtime State ---

        private CameraRig _rig;
        private CameraFPSProfile _activeProfile;
        private InputAction _boundAction;
        private bool _isActive = false;

        private Vector3 _frozenPosition;
        private float _frozenFov;
        private float _pitch;
        private float _yaw;
        private float _originPitch;
        private float _originYaw;
        private float _smoothPitch;
        private float _smoothYaw;

        private float _fpsDuration;
        private float _fpsTimer;

        private float _exitDuration;
        private AnimationCurve _exitCurve;

        // --- Properties ---

        public int Priority => 20;
        public bool IsActive => _isActive;

        // --- ICameraModule Implementation ---

        public void Initialize(CameraRig InRig)
        {
            _rig = InRig;
        }

        public void OnUpdate(float InDeltaTime)
        {
            if (!_isActive || _activeProfile == null)
                return;

            // Auto-exit timer
            if (_fpsDuration > 0f)
            {
                _fpsTimer -= InDeltaTime;
                if (_fpsTimer <= 0f)
                {
                    DisableFPS();
                    return;
                }
            }

            Vector2 lookDelta = ReadLookDelta();

            float rawYaw = lookDelta.x * _activeProfile.sensitivity.x;
            float rawPitch = lookDelta.y * _activeProfile.sensitivity.y;

            if (_activeProfile.invertY)
                rawPitch = -rawPitch;

            _yaw += rawYaw;
            _pitch -= rawPitch;

            _pitch = Mathf.Clamp(_pitch, _originPitch + _activeProfile.pitchLimits.x, _originPitch + _activeProfile.pitchLimits.y);

            if (!_activeProfile.IsYawUnlimited)
                _yaw = Mathf.Clamp(_yaw, _originYaw + _activeProfile.yawLimits.x, _originYaw + _activeProfile.yawLimits.y);

            if (_activeProfile.inputSmoothing > 0.01f)
            {
                float t = 1f - Mathf.Exp(-_activeProfile.inputSmoothing * InDeltaTime);
                _smoothPitch = Mathf.Lerp(_smoothPitch, _pitch, t);
                _smoothYaw = Mathf.Lerp(_smoothYaw, _yaw, t);
            }
            else
            {
                _smoothPitch = _pitch;
                _smoothYaw = _yaw;
            }
        }

        // --- Public API ---

        /// <summary>
        /// Activates FPS mode with the given profile.
        /// Freezes position, captures rotation as Euler angles,
        /// locks the cursor, and pauses the PlayableDirector.
        /// </summary>
        /// <param name="InPosition">Frozen world position for the camera.</param>
        /// <param name="InRotation">Initial rotation to decompose into Pitch/Yaw.</param>
        /// <param name="InProfile">FPS profile containing all behavior parameters.</param>
        /// <param name="InDuration">Duration of FPS mode in seconds. 0 = infinite (exit only via DisableFPS).</param>
        /// <param name="InExitDuration">Duration of the transition back to rail.</param>
        /// <param name="InExitCurve">Easing curve for the exit transition.</param>
        public void EnableFPS(Vector3 InPosition, Quaternion InRotation, CameraFPSProfile InProfile, float InDuration = 0f, float InExitDuration = 1f, AnimationCurve InExitCurve = null)
        {
            if (_isActive)
                return;

            if (InProfile == null)
            {
                Debug.LogWarning("<color=#ff6600><b>[CameraTool]</b></color> FPS Mode: No profile assigned. Aborting.");
                return;
            }

            _activeProfile = InProfile;
            _frozenPosition = InPosition;
            _frozenFov = InProfile.fovOverride > 0f
                ? InProfile.fovOverride
                : (_rig != null && _rig.TargetCamera ? _rig.TargetCamera.fieldOfView : 60f);

            Vector3 euler = InRotation.eulerAngles;
            _yaw = euler.y;
            _pitch = euler.x;

            if (_pitch > 180f)
                _pitch -= 360f;

            _originPitch = _pitch;
            _originYaw = _yaw;

            _smoothPitch = _pitch;
            _smoothYaw = _yaw;

            _fpsDuration = InDuration;
            _fpsTimer = InDuration;

            _exitDuration = InExitDuration;
            _exitCurve = InExitCurve;

            BindLookAction();

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (_rig != null && _rig.Sequences != null && _rig.Sequences.Director != null)
                _rig.Sequences.Director.Pause();

            _isActive = true;

            Debug.Log($"<color=#1ebfff><b>[CameraTool]</b></color> FPS Mode ENABLED — Profile: '{InProfile.name}' (Duration: {(InDuration > 0f ? $"{InDuration}s" : "∞")})");
        }

        /// <summary>
        /// Deactivates FPS mode with a reverse transition back to the rail pose.
        /// The Timeline resumes only AFTER the transition completes.
        /// </summary>
        public void DisableFPS()
        {
            if (!_isActive)
                return;

            _isActive = false;

            UnbindLookAction();

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (_exitDuration > 0.01f && _rig != null && _rig.Transitions != null)
            {
                CameraPose railPose = _rig.GetTrueTargetPose();

                _rig.Transitions.StartTransition(railPose, _exitDuration, _exitCurve);
                StartCoroutine(WaitThenResumeTimeline(_exitDuration));
            }
            else
            {
                if (_rig != null && _rig.Sequences != null && _rig.Sequences.Director != null)
                    _rig.Sequences.Director.Resume();
            }

            Debug.Log($"<color=#1ebfff><b>[CameraTool]</b></color> FPS Mode DISABLED (Exit transition: {_exitDuration}s)");
            _activeProfile = null;
        }

        private System.Collections.IEnumerator WaitThenResumeTimeline(float InDelay)
        {
            yield return new WaitForSeconds(InDelay);

            if (_rig != null && _rig.Transitions != null)
                _rig.Transitions.ReturnToRail(5f);

            if (_rig != null && _rig.Sequences != null && _rig.Sequences.Director != null)
                _rig.Sequences.Director.Resume();

            Debug.Log("<color=#1ebfff><b>[CameraTool]</b></color> Timeline RESUMED after FPS exit transition");
        }

        /// <summary>
        /// Overwrites the incoming pose with the frozen position and input-driven rotation.
        /// Called by CameraRig.ApplyFinalPose() when this module IsActive.
        /// </summary>
        public CameraPose ModifyPose(CameraPose InBasePose)
        {
            InBasePose.position = _frozenPosition;
            InBasePose.rotation = Quaternion.Euler(_smoothPitch, _smoothYaw, 0f);
            InBasePose.fov = _frozenFov;

            return InBasePose;
        }

        // --- Internal ---

        private Vector2 ReadLookDelta()
        {
            // Priority 1: Bound InputAction from the profile
            if (_boundAction != null && _boundAction.enabled)
                return _boundAction.ReadValue<Vector2>() * 0.1f;

            // Priority 2: Fallback to Mouse.current.delta
            if (Mouse.current != null)
                return Mouse.current.delta.ReadValue() * 0.1f;

            return Vector2.zero;
        }

        private void BindLookAction()
        {
            if (_activeProfile == null || _activeProfile.lookAction == null)
                return;

            _boundAction = _activeProfile.lookAction.action;
            if (_boundAction != null && !_boundAction.enabled)
                _boundAction.Enable();
        }

        private void UnbindLookAction()
        {
            if (_boundAction != null)
            {
                _boundAction = null;
            }
        }
    }
}
