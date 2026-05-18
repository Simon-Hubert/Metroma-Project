using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Metroma.CameraTool.Timeline;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using NaughtyAttributes;

namespace Metroma.CameraTool.Modules
{
    /// <summary>
    /// Module responsible for high-level sequencing: Chapters, PlayableDirectors, and Markers.
    /// </summary>
    public class SequenceModule : MonoBehaviour, ICameraModule, INotificationReceiver
    {
        #region --- Serialized Fields ---

        [SerializeField] private PlayableDirector playableDirector;
        [SerializeField] private bool playOnStart = true;
        [SerializeField] private List<CameraChapter> chapters = new List<CameraChapter>();

        [Header("Events (Chapters)")]
        public UnityEvent<CameraChapter> onChapterStart;
        public UnityEvent<CameraChapter> onChapterEnd;

        [Header("Events (FocusCam)")]
        public UnityEvent onFocusStarted;
        public UnityEvent onFocusEnded;

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
        private CameraChapter _activeChapter;
        private int _selectedChapterIndex = 0;
        
        private bool _isWaitingForTransitionToPlay = false;
        private float _transitionWaitTimer = 0f;

        #endregion

        #region --- Properties ---

        public int Priority => 5;
        public bool IsActive => true;
        
        /// <summary> If true, the rail progress is frozen and cannot be modified by the Timeline. </summary>
        public bool IsProgressLocked { get; set; }
        private float _lockedProgress = 0f;

        /// <summary> True if a FocusCam sequence is currently running. </summary>
        public bool IsPlayingFocus { get; private set; }

        public void LockRail()
        {
            IsProgressLocked = true;
            if (_rig != null && _rig.Rails != null)
                _lockedProgress = _rig.Rails.GlobalProgress;
        }

        public void UnlockRail() => IsProgressLocked = false;
        
        public List<CameraChapter> Chapters => chapters;
        public CameraChapter ActiveChapter => _activeChapter;
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

        private void Start()
        {
            if (playOnStart && chapters.Count > 0)
            {
                PlayChapter(0);
            }

#if UNITY_EDITOR
            if (UnityEditor.EditorPrefs.GetBool("AutoTestFocusCam_Pending", false))
            {
                UnityEditor.EditorPrefs.SetBool("AutoTestFocusCam_Pending", false);
                StartCoroutine(DelayedStartTest());
            }
#endif
        }

#if UNITY_EDITOR
        private IEnumerator DelayedStartTest()
        {
            yield return new WaitForSeconds(0.2f);
            EditorTestFocusTimeline();
            UnityEditor.Selection.activeGameObject = gameObject;
            UnityEditor.EditorApplication.ExecuteMenuItem("Window/Sequencing/Timeline");
        }
#endif

        public void OnUpdate(float InDeltaTime)
        {
            if (IsProgressLocked)
            {
                if (_rig != null && _rig.Rails != null)
                    _rig.Rails.GlobalProgress = _lockedProgress;
                return;
            }

            if (_isWaitingForTransitionToPlay)
            {
                _transitionWaitTimer -= InDeltaTime;
                
                if (playableDirector)
                {
                    playableDirector.time = 0f;
                    playableDirector.Evaluate();
                }

                if (_transitionWaitTimer <= 0f)
                {
                    _isWaitingForTransitionToPlay = false;
                }
            }

            if (playableDirector && playableDirector.state == PlayState.Playing)
            {
                double duration = playableDirector.duration;
                if (duration <= 0.01) duration = (_activeChapter != null && _activeChapter.timeline != null) ? _activeChapter.timeline.duration : 1.0;
                
                float progress = Mathf.Clamp01((float)(playableDirector.time / duration));

                _rig.Rails.GlobalProgress = progress;
            }
        }

        #endregion

        #region --- Logic ---

        public void PlayChapter(int InIndex, float InBlendDuration = 1.5f, float InSmoothReturn = 5f)
        {
            if (InIndex < 0 || InIndex >= chapters.Count)
                return;

            CameraChapter chapter = chapters[InIndex];
            if (!chapter.timeline || !playableDirector)
                return;

            if (_activeChapter != null)
                onChapterEnd?.Invoke(_activeChapter);

            _activeChapter = chapter;
            _selectedChapterIndex = InIndex;

            playableDirector.playableAsset = chapter.timeline;
            
            playableDirector.time = 0.05f; 
            playableDirector.Evaluate();
            CameraPose targetPose = _rig.GetTrueTargetPose();
            
            playableDirector.time = 0f;
            playableDirector.Evaluate();

            if (InBlendDuration > 0.01f)
            {
                _rig.Transitions.StartTransition(targetPose, InBlendDuration, null, InSmoothReturn);
                
                _isWaitingForTransitionToPlay = true;
                _transitionWaitTimer = InBlendDuration;
                
                playableDirector.Play();
            }
            else
            {
                playableDirector.Play();
                _rig.Transitions.ReturnToRail(1.0f);
            }

            onChapterStart?.Invoke(chapter);
            _rig.Internal_NotifyChapterStarted(chapter);
        }

        public void PlayChapter(string InChapterName, float InBlendDuration = 1.5f, float InSmoothReturn = 5f)
        {
            int idx = chapters.FindIndex(c => c.name.Equals(InChapterName, System.StringComparison.OrdinalIgnoreCase));
            if (idx >= 0)
            {
                PlayChapter(idx, InBlendDuration, InSmoothReturn);
            }
        }

        public void OnNotify(Playable origin, INotification notification, object context)
        {
            if (!Application.isPlaying)
                return;

            if (_rig == null)
                _rig = GetComponent<CameraRig>();
            
            if (_rig == null)
                return;

            if (notification is CameraMarkerBase marker)
            {
                Debug.Log($"[SequenceModule] Received marker notification: {marker.GetType().Name} from {origin.GetGraph().GetResolver()}");
                if (IsPlayingFocus)
                {
                    string markerName = marker.GetType().Name;
                    if (markerName == "CameraNextChapterMarker" || 
                        markerName == "CameraRailSwitchMarker" || 
                        markerName == "CameraFocusCamMarker" || 
                        markerName == "CameraLookAtSwitchMarker")
                    {
                        Debug.LogWarning($"[SequenceModule] Marker '{markerName}' is blocked during a Focus Cam Timeline to prevent structural conflicts.");
                        return;
                    }
                }

                marker.Execute(_rig, origin.GetGraph().GetResolver());
                _rig.Internal_NotifyMarkerHit(marker);
            }
        }

        public void Internal_NotifyFocusStarted()
        {
            IsPlayingFocus = true;
            onFocusStarted?.Invoke();
            OnFocusStarted?.Invoke();
        }

        public void Internal_NotifyFocusEnded()
        {
            IsPlayingFocus = false;
            onFocusEnded?.Invoke();
            OnFocusEnded?.Invoke();
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
            CameraPose initialPose = new CameraPose 
            { 
                position = _rig.CameraTransform.position, 
                rotation = _rig.CameraTransform.rotation, 
                fov = _rig.TargetCamera ? _rig.TargetCamera.fieldOfView : 60f,
                up = _rig.CameraTransform.up 
            };

            Internal_NotifyFocusStarted();
            InStart?.Invoke();

            CameraPose targetPose = _rig.GetTrueTargetPose();
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
                if (_rig.CameraTransform != null)
                {
                    _rig.CameraTransform.SetPositionAndRotation(targetPose.position, targetPose.rotation);
                    if (_rig.TargetCamera != null) _rig.TargetCamera.fieldOfView = targetPose.fov;
                }
            }

            _rig.SetControlActive(false);
            _rig.Transitions.ClearTransition();

            InDirector.playableAsset = InTimeline;

            // Ensure the Director's GameObject can forward intrinsic markers to the SequenceModule
            CameraMarkerForwarder forwarder = InDirector.GetComponent<CameraMarkerForwarder>();
            if (forwarder == null)
            {
                forwarder = InDirector.gameObject.AddComponent<CameraMarkerForwarder>();
            }
            forwarder.TargetModule = this;

            // 1. Bind the intrinsic Timeline marker track
            if (InTimeline.markerTrack != null)
            {
                Debug.Log("[SequenceModule] Found intrinsic MarkerTrack. Binding to rig.");
                InDirector.SetGenericBinding(InTimeline.markerTrack, _rig.gameObject);
            }
            else
            {
                Debug.LogWarning("[SequenceModule] Intrinsic MarkerTrack is NULL!");
            }

            // 2. Bind any additional user-created marker/tool tracks
            foreach (var track in InTimeline.GetOutputTracks())
            {
                Debug.Log($"[SequenceModule] Found track: {track.name} ({track.GetType().Name})");
                if (track is UnityEngine.Timeline.MarkerTrack)
                {
                    Debug.Log($"[SequenceModule] Binding user MarkerTrack {track.name} to rig.");
                    InDirector.SetGenericBinding(track, _rig.gameObject);
                }
                else if (track.GetType().Name == "CameraToolTrack" || track.GetType().Name == "FocusCamTrack")
                {
                    Debug.Log($"[SequenceModule] Binding Tool/Focus Track {track.name} to rig.");
                    InDirector.SetGenericBinding(track, _rig);
                }
            }

            InDirector.RebuildGraph();
            InDirector.time = 0; // Ensure it starts from the beginning
            InDirector.Play();
            // Removed InDirector.Evaluate() to prevent swallowing timeline notifications on the first frame

            yield return null;
            while (InDirector.state == PlayState.Playing && InDirector.time < InDirector.duration - 0.02f)
            {
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
                    if (_rig.CameraTransform != null)
                    {
                        _rig.CameraTransform.SetPositionAndRotation(initialPose.position, initialPose.rotation);
                        if (_rig.TargetCamera != null) _rig.TargetCamera.fieldOfView = initialPose.fov;
                    }
                }
                
                // Return full control to Rig's usual evaluation system (Rail, FPS, Timeline)
                _rig.SetControlActive(true);
                _rig.Transitions.ReturnToRail(5f);
            }
            else
            {
                _rig.Transitions.SnapToPose(targetPose);
            }

            Internal_NotifyFocusEnded();
            InEnd?.Invoke();
        }

        #endregion

        #region --- Editor Access ---

        public void EditorAddChapter(CameraChapter InChapter) => chapters.Add(InChapter);
        public void EditorClearChapters() => chapters.Clear();

        #endregion

#if UNITY_EDITOR
        #region --- Debug / Test ---

        [Button("🎬 Play Test Focus In-Game")]
        public GameObject EditorTestFocusTimeline()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[SequenceModule] Test Focus only works in Play Mode.");
                return null;
            }

            if (debugFocusTimeline == null)
            {
                Debug.LogError("[SequenceModule] Please assign a 'debugFocusTimeline' asset to test.");
                return null;
            }

            if (playableDirector == null)
            {
                playableDirector = GetComponent<PlayableDirector>();
            }

            if (playableDirector == null)
            {
                Debug.LogError("[SequenceModule] Critical: No Master PlayableDirector found on the CameraRig.");
                return null;
            }

            // 1. Lock the rail progress
            LockRail();

            // 2. Cache original timeline state
            PlayableAsset originalTimeline = playableDirector.playableAsset;
            double originalTime = playableDirector.time;
            PlayState originalState = playableDirector.state;

            // 3. Hijack the master director
            playableDirector.playableAsset = debugFocusTimeline;
            bool shouldReturnToRail = (originalTimeline != null && originalState == PlayState.Playing);
            
            // Auto-bind track to CameraRig
            foreach (var track in debugFocusTimeline.GetOutputTracks())
            {
                if (track is Metroma.FocusCam.FocusCamTrack)
                {
                    playableDirector.SetGenericBinding(track, _rig);
                }
            }

            // 4. Play standalone
            PlayFocusStandalone(playableDirector, debugFocusTimeline, debugBlendIn, debugBlendOut, shouldReturnToRail, null, () => 
            {
                // 5. Restore state
                if (shouldReturnToRail)
                {
                    if (playableDirector != null)
                    {
                        playableDirector.playableAsset = originalTimeline;
                        playableDirector.time = originalTime;
                        if (originalState == PlayState.Playing) playableDirector.Play();
                        else playableDirector.Pause();
                    }
                    
                    UnlockRail();
                    Debug.Log("[SequenceModule] FocusCam test complete. Master Timeline restored.");
                }
            });

            return gameObject;
        }

        #endregion
#endif
    }

    /// <summary>
    /// Forwards Timeline INotifications from an external PlayableDirector to the main SequenceModule.
    /// This is required because Unity routes intrinsic Timeline markers exclusively to the GameObject hosting the PlayableDirector.
    /// </summary>
    public class CameraMarkerForwarder : MonoBehaviour, INotificationReceiver
    {
        public SequenceModule TargetModule;

        public void OnNotify(Playable origin, INotification notification, object context)
        {
            Debug.Log($"[CameraMarkerForwarder] Intercepted notification: {notification.GetType().Name}. Forwarding to {TargetModule}");
            if (TargetModule != null)
            {
                TargetModule.OnNotify(origin, notification, context);
            }
        }
    }
}
