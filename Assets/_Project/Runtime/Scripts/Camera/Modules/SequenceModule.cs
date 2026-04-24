using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Metroma.CameraTool.Timeline;
using System.Collections.Generic;
using UnityEngine.Events;

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
        }

        private void Start()
        {
            if (playOnStart && chapters.Count > 0)
            {
                PlayChapter(0);
            }
        }

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
                marker.Execute(_rig, origin.GetGraph().GetResolver());
                _rig.Internal_NotifyMarkerHit(marker);
            }
        }

        public void Internal_NotifyFocusStarted()
        {
            onFocusStarted?.Invoke();
            OnFocusStarted?.Invoke();
        }

        public void Internal_NotifyFocusEnded()
        {
            onFocusEnded?.Invoke();
            OnFocusEnded?.Invoke();
        }

        /// <summary>
        /// Plays a Focus Timeline independently from the camera rail/spline logic.
        /// Useful for script-triggered cinematics (interactions, events).
        /// </summary>
        public void PlayFocusStandalone(PlayableDirector InDirector, TimelineAsset InTimeline, float InBlendIn = 1f, float InBlendOut = 1f, System.Action InOnStart = null, System.Action InOnEnd = null)
        {
            if (!InDirector || !InTimeline) return;
            _rig.StartCoroutine(StandaloneFocusCoroutine(InDirector, InTimeline, InBlendIn, InBlendOut, InOnStart, InOnEnd));
        }

        private System.Collections.IEnumerator StandaloneFocusCoroutine(PlayableDirector InDirector, TimelineAsset InTimeline, float InIn, float InOut, System.Action InStart, System.Action InEnd)
        {
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

            _rig.SetControlActive(false);
            _rig.Transitions.ClearTransition();

            InDirector.playableAsset = InTimeline;
            InDirector.Play();

            yield return null;
            while (InDirector.state == PlayState.Playing && InDirector.time < InDirector.duration - 0.02f)
            {
                yield return null;
            }

            _rig.SetControlActive(true);
            if (InOut > 0.01f)
            {
                CameraPose railPose = _rig.GetTrueTargetPose();
                _rig.Transitions.StartTransition(railPose, InOut);
                yield return new WaitForSeconds(InOut);
            }
            _rig.Transitions.ReturnToRail(5f);

            Internal_NotifyFocusEnded();
            InEnd?.Invoke();
        }

        #endregion

        #region --- Editor Access ---

        public void EditorAddChapter(CameraChapter InChapter) => chapters.Add(InChapter);
        public void EditorClearChapters() => chapters.Clear();

        #endregion
    }
}
