using System.Collections;
using System.ComponentModel;
using Metroma.CameraTool;
using Metroma.CameraTool.Modules;
using Metroma.FocusCam;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using NaughtyAttributes;

namespace Metroma.CameraTool.Timeline
{
    [DisplayName("FocusCam/🎥 Play Focus Timeline (Auto)")]
    public class CameraFocusCamMarker : CameraMarkerBase
    {
        [Header("Animation")]
        public TimelineAsset focusTimeline;

        [Header("Transition In")]
        public float blendDuration = 1.0f;
        public AnimationCurve blendCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Transition Out")]
        public float exitTransitionDuration = 1.0f;
        public AnimationCurve exitTransitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Settings")]
        public bool returnToLastPos = true;
        public bool pauseMainTimeline = true;
        public bool restartIfPlaying = true;

        [Button("🎮 Preview Sequence")]
        private void EditorPreview()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[CameraFocusCamMarker] Preview only works in Play Mode.");
                return;
            }

            CameraRig rig = CameraRig.Active;
            if (rig != null)
            {
                Execute(rig, null);
            }
        }

        public override void Execute(CameraRig rig, IExposedPropertyTable resolver)
        {
            if (rig == null || focusTimeline == null)
                return;

            // 🛡️ SAFETY: Prevent re-triggering during active lock
            if (rig.Sequences != null && rig.Sequences.IsProgressLocked)
                return;

            PlayableDirector mainDirector = rig.Sequences != null ? rig.Sequences.Director : null;
            
            // 🛡️ DYNAMIC CREATION: We now always create a clean temp director on a child object
            GameObject tempPlayerObject = new GameObject("[Temp_FocusCamPlayer]");
            tempPlayerObject.transform.SetParent(rig.transform);
            
            PlayableDirector focusDirector = tempPlayerObject.AddComponent<PlayableDirector>();
            if (focusDirector != null)
            {
                focusDirector.playOnAwake = false;
                focusDirector.extrapolationMode = DirectorWrapMode.None;
                focusDirector.playableAsset = focusTimeline;

                // 🔗 AUTO-BINDING: Ensure the FocusCam track is bound to the current rig
                foreach (var track in focusTimeline.GetOutputTracks())
                {
                    if (track is FocusCamTrack)
                    {
                        focusDirector.SetGenericBinding(track, rig);
                    }
                }

                CameraPose startPose = ExtractFirstClipPose(rig);
                rig.StartCoroutine(FocusSequenceCoroutine(rig, focusDirector, startPose, mainDirector, tempPlayerObject));
            }
        }

        private CameraPose ExtractFirstClipPose(CameraRig rig)
        {
            CameraPose pose = rig.GetTrueTargetPose(); 
            if (focusTimeline == null) return pose;

            foreach (var track in focusTimeline.GetOutputTracks())
            {
                if (track is FocusCamTrack focusTrack)
                {
                    foreach (var clip in focusTrack.GetClips())
                    {
                        if (clip.asset is FocusCamClip focusAsset)
                        {
                            if (focusAsset.overridePosition) pose.position = focusAsset.cameraPosition;
                            
                            if (focusAsset.mode == FocusMode.LookAtPoint)
                            {
                                Vector3 direction = (focusAsset.position - pose.position).normalized;
                                if (direction != Vector3.zero) pose.rotation = Quaternion.LookRotation(direction, Vector3.up);
                            }
                            else pose.rotation = Quaternion.Euler(focusAsset.rotation);

                            if (focusAsset.overrideFOV) pose.fov = focusAsset.fov;
                            pose.rotation *= Quaternion.Euler(0, 0, focusAsset.roll);
                            
                            return pose;
                        }
                    }
                }
            }
            return pose;
        }

        private IEnumerator FocusSequenceCoroutine(CameraRig rig, PlayableDirector focusDirector, CameraPose targetPose, PlayableDirector mainDirector, GameObject tempObject)
        {
            yield return null;

            // 0. TRIGGER START DELEGATE
            if (rig.Sequences != null) rig.Sequences.Internal_NotifyFocusStarted();

            // 1. LOCK & PAUSE
            if (pauseMainTimeline && mainDirector != null && rig.Sequences != null)
            {
                rig.Sequences.LockRail();
                mainDirector.Pause();
            }

            // 2. TRANSITION IN
            if (blendDuration > 0.01f)
            {
                rig.Transitions.StartTransition(targetPose, blendDuration, blendCurve);
                yield return new WaitForSeconds(blendDuration);
            }
            else
            {
                rig.Transitions.SnapToPose(targetPose);
                if (rig.CameraTransform != null)
                {
                    rig.CameraTransform.SetPositionAndRotation(targetPose.position, targetPose.rotation);
                    if (rig.TargetCamera != null) rig.TargetCamera.fieldOfView = targetPose.fov;
                }
            }

            // 3. HAND OVER TO FOCUS
            rig.SetControlActive(false);
            rig.Transitions.ClearTransition();

            if (focusDirector != null)
            {
                if (restartIfPlaying) focusDirector.Stop();
                focusDirector.Play();
                focusDirector.Evaluate();

                // 5. WAIT FOR FINISH
                yield return null; 
                while (focusDirector != null && focusDirector.state == PlayState.Playing && focusDirector.time < focusDirector.duration - 0.02f)
                {
                    yield return null;
                }
            }

            if (returnToLastPos)
            {
                // 6. RETURN CONTROL
                rig.SetControlActive(true);

                // 7. TRANSITION OUT
                if (exitTransitionDuration > 0.01f)
                {
                    CameraPose railPose = rig.GetTrueTargetPose();
                    rig.Transitions.StartTransition(railPose, exitTransitionDuration, exitTransitionCurve);
                    yield return new WaitForSeconds(exitTransitionDuration);
                }
                else
                {
                    CameraPose railPose = rig.GetTrueTargetPose();
                    rig.Transitions.SnapToPose(railPose);
                    if (rig.CameraTransform != null)
                    {
                        rig.CameraTransform.SetPositionAndRotation(railPose.position, railPose.rotation);
                        if (rig.TargetCamera != null) rig.TargetCamera.fieldOfView = railPose.fov;
                    }
                }
                rig.Transitions.ReturnToRail(5f);
            }
            else
            {
                rig.Transitions.SnapToPose(targetPose);
            }

            // 8. RESUME & UNLOCK
            if (pauseMainTimeline && mainDirector != null && rig.Sequences != null)
            {
                mainDirector.time += 0.05f; 
                mainDirector.Play();
                yield return null; 
                rig.Sequences.UnlockRail();
            }
            
            // 9. TRIGGER END DELEGATE
            if (rig.Sequences != null) rig.Sequences.Internal_NotifyFocusEnded();

            // 10. CLEANUP
            if (tempObject != null)
            {
                Destroy(tempObject);
            }
        }
    }
}