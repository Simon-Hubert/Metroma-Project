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
        [SerializeField] private List<CameraChapter> chapters = new List<CameraChapter>();

        [Header("Events")]
        public UnityEvent<CameraChapter> onChapterStart;
        public UnityEvent<CameraChapter> onChapterEnd;

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

        public void OnUpdate(float InDeltaTime)
        {
            if (_isWaitingForTransitionToPlay)
            {
                _transitionWaitTimer -= InDeltaTime;
                
                // Freeze the playhead at the start to keep the Mixer active and driving the rig
                if (playableDirector)
                {
                    playableDirector.time = 0f;
                    playableDirector.Evaluate();
                }

                if (_transitionWaitTimer <= 0f)
                {
                    _isWaitingForTransitionToPlay = false;
                    // No need to call Play() here as it's already playing, just let it roll
                }
            }

            if (playableDirector && playableDirector.state == PlayState.Playing)
            {
                playableDirector.Evaluate();

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
            
            // Eval slightly ahead to get the target pose
            playableDirector.time = 0.05f; 
            playableDirector.Evaluate();
            CameraPose targetPose = _rig.GetTrueTargetPose();
            
            // Reset to 0 for the blend period
            playableDirector.time = 0f;
            playableDirector.Evaluate();

            if (InBlendDuration > 0.01f)
            {
                _rig.Transitions.StartTransition(targetPose, InBlendDuration, null, InSmoothReturn);
                
                _isWaitingForTransitionToPlay = true;
                _transitionWaitTimer = InBlendDuration;
                
                // Start playing now, but we'll freeze its time in OnUpdate
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
            // Early return if not playing to prevent markers from executing logic in the Editor (e.g., during clip generation)
            if (!Application.isPlaying)
                return;

            if (_rig == null)
                _rig = GetComponent<CameraRig>();
            
            if (_rig == null)
                return;

            if (notification is CameraMarkerBase marker)
            {
                marker.Execute(_rig);
                _rig.Internal_NotifyMarkerHit(marker);
            }
        }

        #endregion

        #region --- Editor Access ---

        public void EditorAddChapter(CameraChapter InChapter) => chapters.Add(InChapter);
        public void EditorClearChapters() => chapters.Clear();

        #endregion
    }
}
