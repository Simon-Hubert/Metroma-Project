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
            {
                Debug.LogWarning($"[CameraFocusCamMarker] Missing Rig ({rig != null}) or { (focusTimeline == null ? "Timeline (NULL)" : "Timeline (OK)") } on marker.");
                return;
            }

            Debug.Log($"[CameraFocusCamMarker] Executing Focus Timeline: {focusTimeline.name}");

            PlayableDirector mainDirector = rig.Sequences != null ? rig.Sequences.Director : null;
            
            // DYNAMIC CREATION: We now always create a clean temp director on a child object
            GameObject tempPlayerObject = new GameObject($"[FocusPlayer_{focusTimeline.name}]");
            tempPlayerObject.transform.SetParent(rig.transform);
            
            PlayableDirector focusDirector = tempPlayerObject.AddComponent<PlayableDirector>();
            
            // FORWARDER: Crucial for markers inside the sub-timeline to work!
            if (rig.Sequences != null)
            {
                var forwarder = tempPlayerObject.AddComponent<CameraMarkerForwarder>();
                forwarder.TargetModule = rig.Sequences;
            }

            if (focusDirector != null)
            {
                focusDirector.playOnAwake = false;
                focusDirector.extrapolationMode = DirectorWrapMode.None;
                focusDirector.playableAsset = focusTimeline;

                // AUTO-BINDING: Ensure all relevant tracks are bound
                foreach (var track in focusTimeline.GetOutputTracks())
                {
                    if (track is FocusCamTrack || track is MarkerTrack)
                    {
                        focusDirector.SetGenericBinding(track, rig);
                    }
                }

                CameraPose targetPose = ExtractFirstClipPose(rig);
                rig.StartCoroutine(FocusSequenceCoroutine(rig, focusDirector, targetPose, mainDirector, tempPlayerObject));
            }
        }

        private CameraPose ExtractFirstClipPose(CameraRig rig)
        {
            // BACK TO MANUAL: More reliable for static analysis than Evaluate() on a fresh object.
            CameraPose pose = rig.GetTrueTargetPose(); 
            if (focusTimeline == null) return pose;

            float earliestStart = float.MaxValue;
            bool foundAny = false;

            foreach (var track in focusTimeline.GetOutputTracks())
            {
                if (track is FocusCamTrack focusTrack)
                {
                    foreach (var clip in focusTrack.GetClips())
                    {
                        if (clip.asset is FocusCamClip focusAsset)
                        {
                            // We want the VERY FIRST clip of the timeline
                            if (clip.start < earliestStart)
                            {
                                earliestStart = (float)clip.start;
                                foundAny = true;

                                Debug.Log($"[CameraFocusCamMarker] Extracting pose from earliest clip: {clip.displayName} at t={clip.start}");
                                
                                if (focusAsset.overridePosition) 
                                {
                                    pose.position = focusAsset.cameraPosition;
                                    Debug.Log($"[CameraFocusCamMarker] -> Position Override: {pose.position}");
                                }
                                
                                if (focusAsset.mode == FocusMode.LookAtPoint)
                                {
                                    Vector3 direction = (focusAsset.position - pose.position).normalized;
                                    if (direction != Vector3.zero) 
                                        pose.rotation = Quaternion.LookRotation(direction, Vector3.up);
                                }
                                else 
                                {
                                    pose.rotation = Quaternion.Euler(focusAsset.rotation);
                                }

                                if (focusAsset.overrideFOV) pose.fov = focusAsset.fov;
                                pose.rotation *= Quaternion.Euler(0, 0, focusAsset.roll);
                            }
                        }
                    }
                }
            }
            
            if (!foundAny) Debug.LogWarning("[CameraFocusCamMarker] No FocusCamClip found to extract pose.");
            return pose;
        }

        private IEnumerator FocusSequenceCoroutine(CameraRig rig, PlayableDirector focusDirector, CameraPose targetPose, PlayableDirector mainDirector, GameObject tempObject)
        {
            // Start immediately to avoid single-frame glitches (Raw Transform for absolute truth)
            CameraPose initialPose = new CameraPose
            {
                position = rig.CameraTransform.position,
                rotation = rig.CameraTransform.rotation,
                fov = rig.TargetCamera ? rig.TargetCamera.fieldOfView : 60f,
                up = rig.CameraTransform.up
            };

            // 0. TRIGGER START DELEGATE
            if (rig.Sequences != null) rig.Sequences.Internal_NotifyFocusStarted();

            // 1. PAUSE MAIN
            if (pauseMainTimeline && mainDirector != null)
                mainDirector.Pause();

            // 2. TRANSITION IN
            if (blendDuration > 0.01f)
            {
                rig.Transitions.StartTransition(targetPose, blendDuration, blendCurve);
                
                float elapsed = 0f;
                while (elapsed < blendDuration)
                {
                    float dt = Time.deltaTime;
                    elapsed += dt;
                    
                    // FORCE Rig Update: This bypasses any pause/stalling issues
                    rig.Internal_ManualUpdate(dt);
                    yield return null;
                }
            }
            else
            {
                rig.Transitions.SnapToPose(targetPose);
            }

            // 3. HAND OVER TO FOCUS
            rig.Transitions.ClearTransition();

            if (focusDirector != null)
            {
                if (restartIfPlaying) focusDirector.Stop();
                focusDirector.Play();
                focusDirector.Evaluate();

                // 5. WAIT FOR FINISH (Robust loop)
                yield return null; 
                float subElapsed = 0f;
                // Wait while playing OR if we just started and state hasn't switched yet
                while (focusDirector != null && (focusDirector.state == PlayState.Playing || subElapsed < 0.2f))
                {
                    subElapsed += Time.deltaTime;
                    if (focusDirector.time >= focusDirector.duration - 0.02f) break;
                    yield return null;
                }
                
                // 1. ULTIMATE SOURCE OF TRUTH: Capture the actual physical transform
                CameraPose exitPose = new CameraPose
                {
                    position = rig.CameraTransform.position,
                    rotation = rig.CameraTransform.rotation,
                    fov = rig.TargetCamera ? rig.TargetCamera.fieldOfView : 60f,
                    up = rig.CameraTransform.up
                };
                
                // 2. Identify return target
                CameraPose target = returnToLastPos ? initialPose : rig.GetTrueTargetPose();
                float duration = Mathf.Max(0.01f, exitTransitionDuration);
                
                // 3. Start Transition and LOCK it immediately
                rig.Transitions.StartTransition(target, duration, exitTransitionCurve);
                rig.Transitions.SetStartPose(exitPose);
                
                // CRITICAL: Stop the sub-timeline influence ONLY after we've locked the pose
                if (focusDirector != null) focusDirector.Stop();
                
                // Force an immediate update to ensure the transition module captures the Rig 
                // and applies the first frame of the blend before Unity can render.
                rig.Internal_ManualUpdate(0); 

                // 4. Smooth Transition Loop
                float outElapsed = 0f;
                while (outElapsed < duration)
                {
                    float dt = Time.deltaTime;
                    outElapsed += dt;
                    rig.Internal_ManualUpdate(dt);
                    yield return null;
                }
                
                // 5. Cleanup
                rig.Transitions.ClearTransition();
            }

            // 8. RESUME
            if (pauseMainTimeline && mainDirector != null)
                mainDirector.Play();
            
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