using Metroma.CameraTool.Modules;
using Metroma.CameraTool.Modifiers;
using UnityEngine.Timeline;
using System;
using System.Collections;
using System.ComponentModel;
using UnityEngine;

namespace Metroma.CameraTool.Timeline
{
    /// <summary>
    /// Timeline Marker that triggers FPS observation mode.
    /// Smoothly transitions to a designer-defined pose, then hands rotation control to the player.
    /// All FPS parameters are read from the assigned <see cref="CameraFPSProfile"/>.
    /// </summary>
    [Serializable]
    [DisplayName("Camera/👁 FPS Observation")]
    [CustomStyle("CameraEventMarker")]
    public class CameraFPSMarker : CameraMarkerBase
    {
        [Tooltip("FPS observation profile. Contains all input, sensitivity, and constraint settings.")]
        [SerializeField] private CameraFPSProfile profile;

        [Tooltip("If true, this marker deactivates FPS mode instead of activating it.")]
        [SerializeField] private bool deactivate = false;

        [Header("Start Pose")]
        [Tooltip("World position where the camera will be placed for FPS observation.")]
        [SerializeField] private Vector3 startPosition;

        [Tooltip("Starting rotation (Euler) for the camera when entering FPS mode.")]
        [SerializeField] private Vector3 startRotation;

        [Header("Transition")]
        [Tooltip("Duration of the smooth transition to the start pose before FPS mode activates.")]
        [SerializeField] private float transitionDuration = 1.0f;

        [Tooltip("Easing curve for the transition into FPS mode.")]
        [SerializeField] private AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Exit Transition")]
        [Tooltip("Duration of the smooth transition back to the rail when exiting FPS mode.")]
        [SerializeField] private float exitTransitionDuration = 1.0f;

        [Tooltip("Easing curve for the exit transition.")]
        [SerializeField] private AnimationCurve exitTransitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Duration")]
        [Tooltip("Duration of FPS mode in seconds. 0 = infinite (exit only via code: rig.FPS.DisableFPS()).")]
        [SerializeField] private float fpsDuration = 0f;

        public override void Execute(CameraRig rig)
        {
            if (rig == null)
                return;

            FPSModule fpsModule = rig.FPS;
            if (fpsModule == null)
                return;

            if (deactivate)
            {
                fpsModule.DisableFPS();
                return;
            }

            if (profile == null)
            {
                Debug.LogWarning("<color=#ff6600><b>[CameraTool]</b></color> CameraFPSMarker: No profile assigned. Skipping.");
                return;
            }

            // Build target pose from marker fields
            CameraPose targetPose = new CameraPose
            {
                position = startPosition,
                rotation = Quaternion.Euler(startRotation),
                fov = profile.fovOverride > 0f ? profile.fovOverride : (rig.TargetCamera ? rig.TargetCamera.fieldOfView : 60f),
                up = Vector3.up
            };

            if (transitionDuration > 0.01f)
            {
                if (rig.Sequences != null && rig.Sequences.Director != null)
                    rig.Sequences.Director.Pause();

                rig.Transitions.StartTransition(targetPose, transitionDuration, transitionCurve);
                rig.StartCoroutine(WaitThenActivateFPS(fpsModule, targetPose));
            }
            else
            {
                fpsModule.EnableFPS(targetPose.position, targetPose.rotation, profile, fpsDuration, exitTransitionDuration, exitTransitionCurve);
            }
        }

        private IEnumerator WaitThenActivateFPS(FPSModule InFPSModule, CameraPose InTargetPose)
        {
            yield return new WaitForSeconds(transitionDuration);

            InFPSModule.EnableFPS(InTargetPose.position, InTargetPose.rotation, profile, fpsDuration, exitTransitionDuration, exitTransitionCurve);
        }
    }
}

