using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Metroma.CameraTool.Timeline;
using Metroma.FocusCam;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using NaughtyAttributes;

namespace Metroma.CameraTool.Modules
{
    /// <summary>
    /// Module responsible for high-level sequencing: FocusCam, PlayableDirectors, and Markers.
    /// FocusCam is now the central system for cinematic sequencing.
    /// </summary>
    public class SequenceModule : MonoBehaviour, ICameraModule, INotificationReceiver
    {
        #region --- Serialized Fields ---

        [SerializeField] private PlayableDirector playableDirector;
        [SerializeField] private bool playOnStart = true;

        [Header("Events (FocusCam)")]
        public UnityEvent onFocusStarted;
        public UnityEvent onFocusEnded;

        [Header("Events (Visibility)")]
        public UnityEvent<string> onVisibilityEvent;
        public System.Action<string> OnVisibilityEvent;

        /// <summary> C# Delegate triggered when any FocusCam sequence starts. </summary>
        public System.Action OnFocusStarted;
        /// <summary> C# Delegate triggered when any FocusCam sequence finishes. </summary>
        public System.Action OnFocusEnded;

#if UNITY_EDITOR
        [Header("🛠️ Debug / Test FocusCam")]
        [SerializeField] private TimelineAsset debugFocusTimeline;
        [SerializeField] private float debugBlendIn = 1f;
        [SerializeField] private float debugBlendOut = 1f;

        public TimelineAsset DebugFocusTimeline => debugFocusTimeline;
#endif

        #endregion

        #region --- Runtime State ---

        private CameraRig _rig;
        private int _lastMarkerFrame = -1;
        private INotification _lastNotification;
        
        #endregion

        #region --- Properties ---

        public int Priority => 5;
        public bool IsActive => true;
        
        /// <summary> True if a FocusCam sequence is currently running. </summary>
        public bool IsPlayingFocus { get; private set; }

        public PlayableDirector Director => playableDirector != null ? playableDirector : (playableDirector = GetComponent<PlayableDirector>());

        #endregion

        #region --- ICameraModule Implementation ---

        public void Initialize(CameraRig InRig)
        {
            _rig = InRig;
            
            if (playableDirector == null)
            {
                playableDirector = GetComponent<PlayableDirector>();
            }

            if (playableDirector != null)
            {
                playableDirector.playOnAwake = false;
                if (!playOnStart)
                {
                    playableDirector.Stop();
                }
            }
        }

        public void OnUpdate(float InDeltaTime)
        {
            // FocusCam logic is handled either by the Timeline Mixer or by StandaloneFocusCoroutine
        }

        #endregion

        #region --- Logic ---

        public void OnNotify(Playable origin, INotification notification, object context)
        {
            if (!Application.isPlaying)
                return;

            if (_rig == null)
                _rig = GetComponent<CameraRig>();
            
            if (_rig == null)
                return;

            // Frame-based cooldown to prevent double-triggering from multiple receivers
            if (Time.frameCount == _lastMarkerFrame && notification == _lastNotification)
                return;

            _lastMarkerFrame = Time.frameCount;
            _lastNotification = notification;

            if (notification is CameraMarkerBase marker)
            {
                marker.Execute(_rig, origin.GetGraph().GetResolver());
                _rig.Internal_NotifyMarkerHit(marker);
            }
            else if (notification is CameraVisibilityMarker visibilityMarker)
            {
                GameObject target = visibilityMarker.targetObject.Resolve(origin.GetGraph().GetResolver());
                if (target != null)
                {
                    StartCoroutine(VisibilityCheckCoroutine(target, visibilityMarker.requiredSeconds, visibilityMarker.eventId, visibilityMarker.checkOcclusion));
                }
            }
        }

        private IEnumerator VisibilityCheckCoroutine(GameObject target, float requiredTime, string eventId, bool checkOcclusion)
        {
            float visibleTimer = 0f;
            
            while (visibleTimer < requiredTime)
            {
                if (IsObjectFullyVisible(target, checkOcclusion))
                {
                    visibleTimer += Time.deltaTime;
                }
                else
                {
                    visibleTimer = 0f;
                }
                
                yield return null;
            }

            // Trigger events
            onVisibilityEvent?.Invoke(eventId);
            OnVisibilityEvent?.Invoke(eventId);
            Debug.Log($"[SequenceModule] Visibility Event Triggered: {eventId}");
        }

        private bool IsObjectFullyVisible(GameObject target, bool checkOcclusion)
        {
            if (target == null || _rig == null || _rig.TargetCamera == null) return false;

            // Get the best bounds (Renderer preferred, then Collider)
            Bounds bounds = new Bounds();
            bool boundsFound = false;
            
            var renderer = target.GetComponentInChildren<Renderer>();
            if (renderer != null) 
            {
                bounds = renderer.bounds;
                boundsFound = true;
            }
            else 
            {
                var collider = target.GetComponentInChildren<Collider>();
                if (collider != null) 
                {
                    bounds = collider.bounds;
                    boundsFound = true;
                }
            }

            if (!boundsFound) return false;

            Camera cam = _rig.TargetCamera;
            Vector3 center = bounds.center;
            Vector3 ext = bounds.extents;

            // Check 8 corners of the AABB in viewport space
            Vector3[] corners = new Vector3[8]
            {
                new Vector3(center.x - ext.x, center.y - ext.y, center.z - ext.z),
                new Vector3(center.x + ext.x, center.y - ext.y, center.z - ext.z),
                new Vector3(center.x - ext.x, center.y + ext.y, center.z - ext.z),
                new Vector3(center.x + ext.x, center.y + ext.y, center.z - ext.z),
                new Vector3(center.x - ext.x, center.y - ext.y, center.z + ext.z),
                new Vector3(center.x + ext.x, center.y - ext.y, center.z + ext.z),
                new Vector3(center.x - ext.x, center.y + ext.y, center.z + ext.z),
                new Vector3(center.x + ext.x, center.y + ext.y, center.z + ext.z)
            };

            foreach (var corner in corners)
            {
                Vector3 viewPos = cam.WorldToViewportPoint(corner);
                // Is corner outside viewport or behind camera?
                if (viewPos.x < 0 || viewPos.x > 1 || viewPos.y < 0 || viewPos.y > 1 || viewPos.z <= 0)
                    return false;
            }

            // Occlusion check: Linecast from camera to center
            if (checkOcclusion)
            {
                if (Physics.Linecast(cam.transform.position, center, out RaycastHit hit))
                {
                    if (hit.collider.gameObject != target && !hit.collider.transform.IsChildOf(target.transform))
                        return false;
                }
            }

            return true;
        }

        public void Internal_NotifyFocusStarted()
        {
            IsPlayingFocus = true;
            onFocusStarted?.Invoke();
            OnFocusStarted?.Invoke();
            _rig.Internal_NotifyStateChanged(CameraState.TimelineDriven);
        }

        public void Internal_NotifyFocusEnded()
        {
            IsPlayingFocus = false;
            onFocusEnded?.Invoke();
            OnFocusEnded?.Invoke();
            _rig.Internal_NotifyStateChanged(CameraState.Manual);
        }

        /// <summary>
        /// Plays a Focus Timeline independently from the camera rail/spline logic.
        /// Useful for script-triggered cinematics (interactions, events).
        /// </summary>
        public void PlayFocusStandalone(PlayableDirector InDirector, TimelineAsset InTimeline, float InBlendIn = 1f, float InBlendOut = 1f, bool InReturnToLastPos = true, System.Action InOnStart = null, System.Action InOnEnd = null)
        {
            if (!InDirector || !InTimeline) return;
            _rig.StartCoroutine(StandaloneFocusCoroutine(InDirector, InTimeline, InBlendIn, InBlendOut, InReturnToLastPos, InOnStart, InOnEnd));
        }

        private System.Collections.IEnumerator StandaloneFocusCoroutine(PlayableDirector InDirector, TimelineAsset InTimeline, float InIn, float InOut, bool InReturnToLastPos, System.Action InStart, System.Action InEnd)
        {
            CameraPose initialPose = _rig.CurrentPose;

            Internal_NotifyFocusStarted();
#if UNITY_EDITOR
            UnityEditor.Selection.activeObject = InDirector;
#endif
            InStart?.Invoke();

            CameraPose targetPose = initialPose; // Fallback
            foreach (var track in InTimeline.GetOutputTracks())
            {
                if (track is Metroma.FocusCam.FocusCamTrack focusTrack)
                {
                    foreach (var clip in focusTrack.GetClips())
                    {
                        if (clip.asset is Metroma.FocusCam.FocusCamClip focusAsset)
                        {
                            if (focusAsset.overridePosition) targetPose.position = focusAsset.cameraPosition;
                            if (focusAsset.mode == Metroma.FocusCam.FocusMode.LookAtPoint)
                            {
                                Vector3 direction = (focusAsset.position - targetPose.position).normalized;
                                if (direction != Vector3.zero) targetPose.rotation = Quaternion.LookRotation(direction, Vector3.up);
                            }
                            else targetPose.rotation = Quaternion.Euler(focusAsset.rotation);

                            if (focusAsset.overrideFOV) targetPose.fov = focusAsset.fov;
                            targetPose.rotation *= Quaternion.Euler(0, 0, focusAsset.roll);
                            goto FoundPose;
                        }
                    }
                }
            }
            FoundPose:

            if (InIn > 0.01f)
            {
                _rig.Transitions.StartTransition(targetPose, InIn);
                yield return new WaitForSeconds(InIn);
            }
            else
            {
                _rig.Transitions.SnapToPose(targetPose);
            }

            _rig.SetControlActive(false);
            _rig.Transitions.ClearTransition();

            InDirector.playableAsset = InTimeline;

            CameraMarkerForwarder forwarder = InDirector.GetComponent<CameraMarkerForwarder>();
            if (forwarder == null)
            {
                forwarder = InDirector.gameObject.AddComponent<CameraMarkerForwarder>();
            }
            forwarder.TargetModule = this;

            // Bind tracks to rig
            // BINDING: Ensure we can receive notifications and apply pose
            foreach (var track in InTimeline.GetOutputTracks())
            {
                if (track is FocusCamTrack || track is MarkerTrack)
                {
                    InDirector.SetGenericBinding(track, _rig);
                }
            }

            InDirector.RebuildGraph();
            InDirector.time = 0; 
            InDirector.Play();
            InDirector.Evaluate(); // Force first frame

            yield return null;
            // Wait until the entire focus sequence is finished (including chained timelines)
            while (InDirector != null && IsPlayingFocus)
            {
                // Safety: if the director is NOT paused and reached the end, we might need to break
                // but only if no other sub-focus has taken over.
                // However, Internal_NotifyFocusEnded() will be called by the last coroutine.
                yield return null;
            }

            if (InReturnToLastPos)
            {
                if (InOut > 0.01f)
                {
                    _rig.Transitions.StartTransition(initialPose, InOut);
                    yield return new WaitForSeconds(InOut);
                }
                else
                {
                    _rig.Transitions.SnapToPose(initialPose);
                }
                
                _rig.SetControlActive(true);
                _rig.Transitions.ReturnToRigControl(5f);
            }

            Internal_NotifyFocusEnded();
            InEnd?.Invoke();
        }

        #endregion

#if UNITY_EDITOR
        #region --- Debug / Test ---

        [Button("🎬 Play Test Focus In-Game")]
        public void EditorTestFocusTimeline()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[SequenceModule] Test Focus only works in Play Mode.");
                return;
            }

            if (debugFocusTimeline == null)
            {
                Debug.LogError("[SequenceModule] Please assign a 'debugFocusTimeline' asset to test.");
                return;
            }

            if (playableDirector == null)
            {
                playableDirector = GetComponent<PlayableDirector>();
            }

            if (playableDirector == null)
                return;

            PlayFocusStandalone(playableDirector, debugFocusTimeline, debugBlendIn, debugBlendOut, true, null, () => 
            {
                Debug.Log("[SequenceModule] FocusCam test complete.");
            });
        }

        #endregion
#endif
    }

    public class CameraMarkerForwarder : MonoBehaviour, INotificationReceiver
    {
        public SequenceModule TargetModule;

        public void OnNotify(Playable origin, INotification notification, object context)
        {
            if (TargetModule != null)
            {
                TargetModule.OnNotify(origin, notification, context);
            }
        }
    }
}
