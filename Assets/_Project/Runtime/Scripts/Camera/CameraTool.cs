using System;
using UnityEngine;
using UnityEngine.Playables;
using Dreamteck.Splines;
using NaughtyAttributes;
using Metroma.CameraTool.Timeline;
using UnityEngine.Timeline;
using UnityEngine.Events;
using System.Collections.Generic;
using Metroma.CameraTool.Modifiers;

namespace Metroma.CameraTool
{
    
    [DisallowMultipleComponent]
    public class CameraTool : MonoBehaviour, INotificationReceiver
    {
        
        #region Service Access
        
        /// <summary> The currently active CameraTool instance in the scene. </summary>
        public static CameraTool Active { get; private set; }

        /// <summary> Triggered when a camera transition starts. </summary>
        public event Action<CameraPose> OnTransitionStarted;
        
        #endregion

        
        #region Serialized Fields
        
        [Foldout("References")]
        [SerializeField] private List<SplineComputer> splineRails = new List<SplineComputer>();

        [Foldout("References")]
        [Required("Assign the Camera to drive along the spline.")]
        [SerializeField] private UnityEngine.Camera targetCamera;

        [Foldout("References")]
        [SerializeField] private PlayableDirector playableDirector;

        [Foldout("References")]
        [Tooltip("Optional: default target to look at.")]
        [SerializeField] private Transform lookAtTarget;

        [Foldout("References")]
        [Tooltip("Optional: list of potential targets to look at. Switch via markers.")]
        [SerializeField] private List<Transform> lookAtTargets = new List<Transform>();

        [Foldout("Chapters")]
        [SerializeField] private List<CameraChapter> chapters = new List<CameraChapter>();

        [Foldout("Events")]
        public UnityEvent<CameraChapter> onChapterStart;
        [Foldout("Events")]
        public UnityEvent<CameraChapter> onChapterEnd;
        [Foldout("Events")]
        public UnityEvent<string> onSplineNotified;

        public event Action<CameraState> OnStateChanged;
        public event Action<CameraChapter> OnChapterStarted;
        public event Action<CameraChapter> OnChapterActive;
        public event Action<CameraPose> OnPoseEventReached;
        public event Action<CameraMarkerBase> OnMarkerEventHit;

        [Foldout("Animation Settings")]
        [Tooltip("If unchecked, the tool releases control and stops overwriting the camera transform.")]
        [SerializeField] private bool autoHandleCamera = true;
        
        public bool IsControlActive => autoHandleCamera;

        [Foldout("Animation Settings")]
        [Range(0f, 1f)]
        [SerializeField] private float splineProgress;

        [Foldout("Animation Settings")]
        [Range(0f, 1f)]
        [Tooltip("Blend between spline rotation (0) and LookAt rotation (1).")]
        [SerializeField] private float lookAtWeight = 1f;

        // --- Persistence (Editor Only) ---
#pragma warning disable 0414
        [SerializeField, HideInInspector] private int selectedChapterIndex = 0;
        [SerializeField, HideInInspector] private bool foldReferences = true;
        [SerializeField, HideInInspector] private bool foldChapters = true;
        [SerializeField, HideInInspector] private bool foldSegments = true;
        [SerializeField, HideInInspector] private bool foldAnimation = true;
        [SerializeField, HideInInspector] private bool foldEvents = false;
        [SerializeField, HideInInspector] private bool foldViewport = false;
        [SerializeField, HideInInspector] private bool foldHaptics = false;
        [SerializeField, HideInInspector] private bool foldDebug = false;
#pragma warning restore 0414

        #endregion

        
        #region Runtime State
        
        private Transform _cameraTransform;
        private bool _isInitialized;

        private SplineComputer[] _chainRails;
        private int[] _chainSegCounts;
        private int _chainTotalSegments;

        private int _activeRailIndex;

        private Transform _lookAtTarget;
        private Transform _lookAtFrom;
        private float _lookAtLerp;
        private float _lookAtDuration;
        private bool _isDrivenByTimeline;
        private CameraPose _timelinePose;
        private bool _playOnTransitionArrival;
        
        private TransitionLookAtMode _transitionLookAtMode;
        private Transform _transitionLookAtTarget;
        private Vector3 _transitionLookAtPoint;
        private float _rotationSmoothness;
        private float _rotationReturnTimer;
        private int _lastEvaluatedRailIndex = -1;

        private CameraState _state = CameraState.FollowRail;
        private CameraChapter _activeChapter;
        private CameraPose _targetPose;
        private CameraPose _startPose;
        private float _transitionTime;
        private float _transitionDuration;
        private AnimationCurve _transitionCurve;
        private float _transitionAlpha;
        private DirectorAction _arrivalAction;

        public CameraState State 
        { 
            get => _state; 
            private set
            {
                if (_state == value)
                    return;
                
                _state = value;
                OnStateChanged?.Invoke(_state);
            }
        }
        #endregion

        
        #region Lifecycle
        
        private void Awake()
        {
            Active = this;
            CacheReferences();
            _lookAtTarget = lookAtTarget;
        }

        private void OnDisable()
        {
            CameraTimeHandler.ResetTimeScale();
        }

        private void OnDestroy()
        {
            if (Active == this)
                Active = null;
        }

        private void LateUpdate()
        {
            if (!_isInitialized || !autoHandleCamera)
            {
                return;
            }

            UpdateEffectsTimers();
            UpdateTransitionState();
            UpdateRotationReturn();

            if (_isDrivenByTimeline)
            {
                if (State == CameraState.Transitioning || State == CameraState.ReturningToRail || _rotationReturnTimer > 0)
                {
                    ApplyCameraPose(_timelinePose);
                }
            }
            else
            {
                CameraPose railPose = SampleRailPose();
                ApplyCameraPose(railPose);
            }
            
            _isDrivenByTimeline = false;
        }
        #endregion

        
        #region Internal Logic
        
        private void CacheReferences()
        {
            if (targetCamera != null)
                _cameraTransform = targetCamera.transform;

            _isInitialized = HasValidRails() && _cameraTransform != null;

            if (_isInitialized)
                RebuildChainCache();
        }

        private bool HasValidRails()
        {
            if (splineRails == null || splineRails.Count == 0)
                return false;
            
            foreach (var r in splineRails)
            {
                if (r)
                    return true;
            }
            
            return false;
        }

        private void RebuildChainCache()
        {
            int validCount = 0;
            foreach (var r in splineRails)
            {
                if (r)
                    validCount++;
            }

            _chainRails = new SplineComputer[validCount];
            _chainSegCounts = new int[validCount];
            _chainTotalSegments = 0;

            int idx = 0;
            foreach (var r in splineRails)
            {
                if (!r)
                    continue;
                
                _chainRails[idx] = r;
                _chainSegCounts[idx] = Mathf.Max(1, r.pointCount - 1);
                _chainTotalSegments += _chainSegCounts[idx];
                idx++;
            }
        }

        private void UpdateEffectsTimers()
        {
            if (_lookAtLerp < 1f && _lookAtDuration > 0)
            {
                _lookAtLerp += Time.deltaTime / _lookAtDuration;
            }
        }

        private void UpdateTransitionState()
        {
            if (State != CameraState.Transitioning && State != CameraState.ReturningToRail)
            {
                return;
            }

            _transitionTime += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(_transitionTime / _transitionDuration);
            _transitionAlpha = _transitionCurve != null ? _transitionCurve.Evaluate(t) : Mathf.SmoothStep(0, 1, t);

            if (t >= 1f)
            {
                HandleTransitionArrival();
            }
        }

        private void UpdateRotationReturn()
        {
            if (_rotationReturnTimer > 0)
            {
                _rotationReturnTimer -= Time.unscaledDeltaTime;
                if (_rotationReturnTimer < 0)
                {
                    _rotationReturnTimer = 0;
                }
            }
        }

        private void HandleTransitionArrival()
        {
            if (State == CameraState.Transitioning)
            {
                State = CameraState.StaticPose;
                
                if (playableDirector)
                {
                    if (_arrivalAction == DirectorAction.Pause)
                    {
                        playableDirector.Pause();
                    }
                    else if (_arrivalAction == DirectorAction.Stop)
                    {
                        playableDirector.Stop();
                    }
                }

                OnPoseEventReached?.Invoke(_targetPose);
            }
            else if (State == CameraState.ReturningToRail)
            {
                _rotationReturnTimer = 2.0f; 
                State = CameraState.FollowRail;
                
                if (_playOnTransitionArrival && playableDirector)
                {
                    playableDirector.Play();
                    _playOnTransitionArrival = false;
                }

                if (_activeChapter != null)
                {
                    OnChapterActive?.Invoke(_activeChapter);
                    _activeChapter = null;
                }
            }

            _arrivalAction = DirectorAction.None;
        }

        /// <summary> Decoupled sampling of the current rail position/rotation/fov. </summary>
        private CameraPose SampleRailPose()
        {
            SplineSample result = EvaluateChainAt(splineProgress);
            
            CameraPose pose = new CameraPose
            {
                position = result.position,
                rotation = result.rotation,
                fov = targetCamera.fieldOfView,
                distance = 0f
            };

            // Apply LookAt logic
            Transform currentTarget = _lookAtTarget;
            if (currentTarget && lookAtWeight > 0.001f)
            {
                Vector3 targetPos = currentTarget.position;
                if (_lookAtLerp < 1f && _lookAtFrom)
                {
                    targetPos = Vector3.Lerp(_lookAtFrom.position, currentTarget.position, Mathf.SmoothStep(0, 1, _lookAtLerp));
                }

                Vector3 direction = targetPos - pose.position;
                if (direction.sqrMagnitude > 0.001f)
                {
                    Quaternion lookRot = Quaternion.LookRotation(direction, result.up);
                    pose.rotation = Quaternion.Slerp(pose.rotation, lookRot, lookAtWeight);
                }
            }

            return pose;
        }

        /// <summary> Final application and blending to camera transform. </summary>
        private void ApplyCameraPose(CameraPose railPose)
        {
            CameraPose finalPose = railPose;

            switch (State)
            {
                case CameraState.Transitioning:
                    finalPose = CameraPose.Lerp(railPose, _targetPose, _transitionAlpha);
                    break;
                
                case CameraState.ReturningToRail:
                    finalPose = CameraPose.Lerp(_startPose, railPose, _transitionAlpha);
                    break;
                
                case CameraState.StaticPose:
                    finalPose = _targetPose;
                    break;
            }

            // ── 1. Calculate the final IDEAL rotation ──
            if (State == CameraState.ReturningToRail && _transitionLookAtMode != TransitionLookAtMode.TimelineDefault)
            {
                Vector3 targetPoint = Vector3.zero;
                bool hasPoint = false;

                if (_transitionLookAtMode == TransitionLookAtMode.SpecificGameObject)
                {
                    if (_transitionLookAtTarget != null)
                    {
                        targetPoint = _transitionLookAtTarget.position;
                        hasPoint = true;
                    }
                }
                else
                {
                    targetPoint = _transitionLookAtPoint;
                    hasPoint = true;
                }

                if (hasPoint)
                {
                    Vector3 lookDir = (targetPoint - finalPose.position).normalized;
                    if (lookDir.sqrMagnitude > 0.001f)
                    {
                        finalPose.rotation = Quaternion.LookRotation(lookDir);
                    }
                }
            }

            // ── 2. Calculate the final IDEAL position (including distance offset) ──
            if (finalPose.distance != 0)
            {
                finalPose.position -= (finalPose.rotation * Vector3.forward) * finalPose.distance;
            }

            // ── 3. Unified Global Smoothing (Glide vs TP) ──
            if (State == CameraState.ReturningToRail || _rotationReturnTimer > 0)
            {
                float t = _rotationSmoothness * Time.unscaledDeltaTime;
                
                _cameraTransform.position = Vector3.Lerp(_cameraTransform.position, finalPose.position, t);
                _cameraTransform.rotation = Quaternion.Slerp(_cameraTransform.rotation, finalPose.rotation, t);
            }
            else
            {
                _cameraTransform.SetPositionAndRotation(finalPose.position, finalPose.rotation);
            }

            targetCamera.fieldOfView = finalPose.fov;
        }

        private SplineSample EvaluateChainAt(float globalProgress)
        {
            if (_chainRails == null || _chainRails.Length == 0)
            {
                return default;
            }

            int startIdx = (_activeChapter != null) ? _activeChapter.startRailIndex : 0;
            int count = (_activeChapter != null) ? _activeChapter.railCount : _chainRails.Length;

            List<CameraSplineSegment> activeSegments = (_activeChapter != null) ? _activeChapter.segments : null;

            if (activeSegments != null && activeSegments.Count > 0)
            {
                return EvaluateWithSegments(globalProgress, activeSegments, startIdx, count);
            }

            return EvaluateLinear(globalProgress, startIdx, count);
        }
        #endregion

        #region Evaluation Engine
        /// <summary> Synchronizes the internal cache for a chapter's durations to optimize frame-by-frame evaluation. </summary>
        private void EnsureChapterCache(int chapterIdx)
        {
            if (chapters == null || chapterIdx < 0 || chapterIdx >= chapters.Count)
            {
                return;
            }

            var chapter = chapters[chapterIdx];
            if (chapter.cachedTotalMoveDuration >= 0f && chapter.cachedRailDurations != null && chapter.cachedRailDurations.Length == chapter.railCount)
            {
                return;
            }

            chapter.cachedTotalMoveDuration = 0f;
            chapter.cachedRailDurations = new float[chapter.railCount];
            chapter.cachedRailSegmentStarts = new int[chapter.railCount];

            int segGlobalIdx = 0;
            
            for (int r = 0; r < chapter.railCount; r++)
            {
                chapter.cachedRailSegmentStarts[r] = segGlobalIdx;
                int railIdx = chapter.startRailIndex + r;
                int sCount = EditorSegmentCountInRail(railIdx);
                float railDur = 0f;

                for (int s = 0; s < sCount; s++)
                {
                    if (segGlobalIdx < chapter.segments.Count)
                    {
                        railDur += chapter.segments[segGlobalIdx++].duration;
                    }
                }

                chapter.cachedRailDurations[r] = railDur;
                chapter.cachedTotalMoveDuration += railDur;
            }
        }

        private SplineSample EvaluateWithSegments(float globalProgress, List<CameraSplineSegment> activeSegments, int railStartIdx, int railCount)
        {
            float totalDuration = 0;
            foreach (var s in activeSegments)
            {
                totalDuration += s.duration;
            }

            if (totalDuration <= 0)
            {
                return EvaluateLinear(globalProgress, railStartIdx, railCount);
            }

            float targetTime = globalProgress * totalDuration;
            float accumulatedTime = 0;

            for (int i = 0; i < activeSegments.Count; i++)
            {
                var seg = activeSegments[i];
                if (targetTime <= accumulatedTime + seg.duration || i == activeSegments.Count - 1)
                {
                    float localT = Mathf.Clamp01((targetTime - accumulatedTime) / seg.duration);
                    float easedT = seg.easing != null ? seg.easing.Evaluate(localT) : localT;
                    
                    return EvaluateAbsSegment(i, easedT, railStartIdx, railCount);
                }
                accumulatedTime += seg.duration;
            }

            return _chainRails[Mathf.Min(railStartIdx + railCount - 1, _chainRails.Length - 1)].Evaluate(1f);
        }

        private SplineSample EvaluateLinear(float globalProgress, int railStartIdx, int railCount)
        {
            int totalSegs = 0;
            int endIdx = Mathf.Min(railStartIdx + railCount, _chainRails.Length);

            for (int r = railStartIdx; r < endIdx; r++)
            {
                totalSegs += _chainSegCounts[r];
            }

            if (totalSegs == 0)
            {
                return _chainRails[Mathf.Clamp(railStartIdx, 0, _chainRails.Length - 1)].Evaluate(globalProgress);
            }

            float segWidth = 1f / totalSegs;
            int targetSeg = Mathf.Clamp((int)(globalProgress / segWidth), 0, totalSegs - 1);
            float segLocalT = Mathf.Clamp01((globalProgress - targetSeg * segWidth) / segWidth);
            return EvaluateAbsSegment(targetSeg, segLocalT, railStartIdx, railCount);
        }

        /// <summary> Evaluates progress (0-1) on a specific rail. Used for Modular Clips. </summary>
        public SplineSample EvaluateRailLocal(int railIdx, float localT)
        {
            if (railIdx < 0 || railIdx >= _chainRails.Length)
            {
                return default;
            }

            return _chainRails[railIdx].Evaluate(localT);
        }

        /// <summary> Statelessly evaluates a camera pose on a specific rail. </summary>
        public CameraPose GetPoseOnRail(int railIdx, float localT, int chapterIdx = -1)
        {
            if (splineRails == null || railIdx < 0 || railIdx >= splineRails.Count || !splineRails[railIdx])
            {
                return default;
            }

            float finalT = localT;

            if (chapterIdx >= 0 && chapterIdx < chapters.Count)
            {
                EnsureChapterCache(chapterIdx);
                var chapter = chapters[chapterIdx];
                int relativeRail = railIdx - chapter.startRailIndex;

                if (relativeRail >= 0 && relativeRail < chapter.railCount && chapter.cachedRailDurations[relativeRail] > 0)
                {
                    float railTotalDur = chapter.cachedRailDurations[relativeRail];
                    float targetTime = localT * railTotalDur;
                    float accum = 0;
                    
                    int segStart = chapter.cachedRailSegmentStarts[relativeRail];
                    int segCount = EditorSegmentCountInRail(railIdx);

                    for (int s = 0; s < segCount; s++)
                    {
                        var seg = chapter.segments[segStart + s];
                        if (targetTime <= accum + seg.duration || s == segCount - 1)
                        {
                            float sLocal = Mathf.Clamp01((targetTime - accum) / seg.duration);
                            float easedS = seg.easing != null ? seg.easing.Evaluate(sLocal) : sLocal;
                            finalT = ((float)s + easedS) / segCount;
                            break;
                        }
                        accum += seg.duration;
                    }
                }
            }

            // ── Intra-Chapter Rail-to-Rail Smoothing (Mixer Version) ──
            // Detect if the Mixer just jumped to a different rail.
            if (_lastEvaluatedRailIndex != -1 && _lastEvaluatedRailIndex != railIdx && Application.isPlaying)
            {
                _rotationReturnTimer = 0.5f;
            }
            _lastEvaluatedRailIndex = railIdx;

            SplineSample sample = splineRails[railIdx].Evaluate(finalT);
            return new CameraPose
            {
                position = sample.position,
                rotation = sample.rotation,
                fov = targetCamera ? targetCamera.fieldOfView : 60f,
                distance = 0f
            };
        }

        /// <summary> Statelessly evaluates a camera pose within a chapter's bounded rail range. </summary>
        public CameraPose GetPoseOnChapter(int chapterIdx, float progress)
        {
            if (chapterIdx < 0 || chapterIdx >= chapters.Count)
            {
                return default;
            }

            RebuildChainCache();
            SplineSample sample = EvaluateWithSegments(Mathf.Clamp01(progress), chapters[chapterIdx].segments, chapters[chapterIdx].startRailIndex, chapters[chapterIdx].railCount);
            return new CameraPose
            {
                position = sample.position,
                rotation = sample.rotation,
                fov = targetCamera ? targetCamera.fieldOfView : 60f,
                distance = 0f
            };
        }

        /// <summary> Converts a local rail progress to a global chapter progress (0-1). </summary>
        public float GetGlobalProgressFromRail(int railIdx, float localT, int chapterIdx)
        {
            if (chapterIdx < 0 || chapterIdx >= chapters.Count)
            {
                return 0f;
            }

            EnsureChapterCache(chapterIdx);
            var chapter = chapters[chapterIdx];
            int relativeRail = railIdx - chapter.startRailIndex;

            if (relativeRail < 0 || relativeRail >= chapter.railCount || chapter.cachedTotalMoveDuration <= 0)
            {
                return 0f;
            }

            float accum = 0;
            for (int r = 0; r < relativeRail; r++)
            {
                accum += chapter.cachedRailDurations[r];
            }

            return (accum + localT * chapter.cachedRailDurations[relativeRail]) / chapter.cachedTotalMoveDuration;
        }

        /// <summary> Directly applies a pose and look-at weight to the camera, overriding internal state evaluation. </summary>
        public void ApplyTimelinePose(CameraPose pose, float weight)
        {
            if (!targetCamera)
            {
                return;
            }

            _isDrivenByTimeline = true;
            _timelinePose = ApplyLookAt(pose, weight);

            if (State == CameraState.Transitioning || State == CameraState.ReturningToRail || _rotationReturnTimer > 0)
            {
                return;
            }
            
            targetCamera.transform.SetPositionAndRotation(_timelinePose.position, _timelinePose.rotation);
            targetCamera.fieldOfView = _timelinePose.fov;

            if (_timelinePose.distance != 0)
            {
                targetCamera.transform.position -= targetCamera.transform.forward * _timelinePose.distance;
            }

#if UNITY_EDITOR
            // Force SceneView updates in Editor during Timeline scrubbing
            if (!Application.isPlaying)
            {
                UnityEditor.EditorUtility.SetDirty(targetCamera.transform);
                UnityEditor.SceneView.RepaintAll();
            }
#endif
        }

        public CameraPose ApplyLookAt(CameraPose basePose, float weight)
        {
            if (weight < 0.001f || !_lookAtTarget)
            {
                return basePose;
            }

            Vector3 direction = (_lookAtTarget.position - basePose.position).normalized;
            if (direction.sqrMagnitude < 0.001f)
            {
                return basePose;
            }

            Quaternion lookRot = Quaternion.LookRotation(direction);
            basePose.rotation = Quaternion.Slerp(basePose.rotation, lookRot, weight);
            
            return basePose;
        }

        private SplineSample EvaluateAbsSegment(int absoluteSegmentIndex, float localT, int railStartIdx, int railCount)
        {
            int running = 0;
            int endIdx = Mathf.Min(railStartIdx + railCount, _chainRails.Length);

            for (int r = railStartIdx; r < endIdx; r++)
            {
                int count = _chainSegCounts[r];
                if (absoluteSegmentIndex < running + count)
                {
                    int localSeg = absoluteSegmentIndex - running;
                    float tStart = (float)localSeg / count;
                    float tEnd = (float)(localSeg + 1) / count;

                    // ── Intra-Chapter Rail-to-Rail Smoothing ──
                    // Detect if we just jumped to a different rail within the same chapter.
                    // If we did, we trigger the common smoothing buffer to ensure a glide instead of a snap.
                    if (_lastEvaluatedRailIndex != -1 && _lastEvaluatedRailIndex != r && Application.isPlaying)
                    {
                        _rotationReturnTimer = 0.5f;
                    }
                    _lastEvaluatedRailIndex = r;

                    return _chainRails[r].Evaluate(Mathf.Lerp(tStart, tEnd, localT));
                }
                running += count;
            }
            return default;
        }
        #endregion

        
        #region Public API
        
        /// <summary> Smoothly transitions the camera away from the rail to a fixed pose. </summary>
        public void TransitionToPose(CameraPose target, float duration, AnimationCurve curve = null, DirectorAction action = DirectorAction.None)
        {
            if (!_cameraTransform)
            {
                CacheReferences();
            }

            _startPose = new CameraPose
            {
                position = _cameraTransform.position,
                rotation = _cameraTransform.rotation,
                fov = targetCamera.fieldOfView,
                distance = 0f
            };

            _targetPose = target;
            _transitionDuration = duration;
            _transitionCurve = curve;
            _transitionTime = 0f;
            _arrivalAction = action;

            if (playableDirector)
            {
                if (_arrivalAction == DirectorAction.Pause)
                {
                    playableDirector.Pause();
                }
                else if (_arrivalAction == DirectorAction.Stop)
                {
                    playableDirector.Stop();
                }
            }
            
            State = CameraState.Transitioning;
            OnTransitionStarted?.Invoke(target);
        }

        /// <summary> Returns control to the spline rail from a static pose. </summary>
        public void ReturnToRail(float duration, AnimationCurve curve = null)
        {
            if (State == CameraState.FollowRail)
            {
                return;
            }

            if (!_cameraTransform)
            {
                CacheReferences();
            }

            _startPose = new CameraPose
            {
                position = _cameraTransform.position,
                rotation = _cameraTransform.rotation,
                fov = targetCamera.fieldOfView,
                distance = 0f
            };

            _transitionDuration = duration;
            _transitionCurve = curve;
            _transitionTime = 0f;

            if (playableDirector != null && playableDirector.state != PlayState.Playing)
            {
                playableDirector.Play();
            }

            State = CameraState.ReturningToRail;
        }

        /// <summary> Plays a specific chapter by swapping the Master Director's asset and re-binding tracks. </summary>
        public void PlayChapter(int index, float blendDuration = 1.5f, TransitionLookAtMode lookAtMode = TransitionLookAtMode.TimelineDefault, Transform lookAtTarget = null, float rotationSmoothness = 5.0f)
        {
            if (index < 0 || index >= chapters.Count)
                return;

            var chapter = chapters[index];
            if (chapter == null || !chapter.timeline || !playableDirector)
                return;

            if (_activeChapter != null) 
                onChapterEnd?.Invoke(_activeChapter);

            // ── 1. Capture the REAL current camera pose BEFORE any changes ──
            if (!_cameraTransform)
            {
                CacheReferences();
            }

            CameraPose currentPose = new CameraPose
            {
                position = _cameraTransform.position,
                rotation = _cameraTransform.rotation,
                fov = targetCamera.fieldOfView,
                distance = 0f
            };

            // ── 2. Swap Asset and Re-bind ──
            if (playableDirector.state == PlayState.Playing)
            {
                playableDirector.Stop();
            }

            playableDirector.playableAsset = chapter.timeline;
            BindTimelineEntries(chapter.timeline);
            playableDirector.extrapolationMode = DirectorWrapMode.Hold;
            
            _activeRailIndex = chapter.startRailIndex;
            _activeChapter = chapter;
            selectedChapterIndex = index;
            _lastEvaluatedRailIndex = -1;

            // ── 3. Initialize Universal Smoothing Parameters ──
            // We set these unconditionally so intra-chapter rail smoothing functions correctly
            // even if the chapter start itself was instant.
            _transitionLookAtMode = lookAtMode;
            _transitionLookAtTarget = lookAtTarget;
            _rotationSmoothness = rotationSmoothness;
            _rotationReturnTimer = 0;

            // ── 4. Initialize Transition Logic ──
            if (blendDuration > 0.01f)
            {
                splineProgress = 0;
                
                _timelinePose = SampleRailPose();

                playableDirector.time = 0;
                playableDirector.Evaluate();
                
                if (lookAtMode == TransitionLookAtMode.ChapterStart)
                {
                    _transitionLookAtPoint = _timelinePose.position;
                }
                else if (lookAtMode == TransitionLookAtMode.ChapterEnd)
                {
                    float oldProgress = splineProgress;
                    splineProgress = 1f;
                    _transitionLookAtPoint = SampleRailPose().position;
                    splineProgress = oldProgress;
                }

                _startPose = currentPose;
                _transitionDuration = blendDuration;
                _transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
                _transitionTime = 0f;
                _playOnTransitionArrival = true;

                State = CameraState.ReturningToRail;
                _cameraTransform.SetPositionAndRotation(_startPose.position, _startPose.rotation);
            }
            else
            {
                splineProgress = 0;
                playableDirector.time = 0;
                playableDirector.Evaluate();
                _playOnTransitionArrival = false;
                State = CameraState.FollowRail;
            }

            OnChapterStarted?.Invoke(chapter);
            onChapterStart?.Invoke(chapter);

            if (!_playOnTransitionArrival)
            {
                playableDirector.Play();
            }
        }

        private void BindTimelineEntries(TimelineAsset asset)
        {
            if (asset == null || playableDirector == null)
                return;

            foreach (var track in asset.GetOutputTracks())
            {
                if (track is CameraToolTrack)
                {
                    playableDirector.SetGenericBinding(track, this);
                    break; 
                }
            }
        }

        public void PlayChapter(string chapterName, float blendDuration = 1.5f, TransitionLookAtMode lookAtMode = TransitionLookAtMode.TimelineDefault, Transform lookAtTarget = null, float rotationSmoothness = 5.0f)
        {
            int idx = chapters.FindIndex(c => c.name.Equals(chapterName, StringComparison.OrdinalIgnoreCase));
            
            if (idx >= 0)
            {
                PlayChapter(idx, blendDuration, lookAtMode, lookAtTarget, rotationSmoothness);
            }
        }

        public void SwitchToRail(int idx)
        {
            _activeRailIndex = idx;
        }

        /// <summary> Toggles whether this tool should actively control the target camera. </summary>
        public void SetControlActive(bool active)
        {
            autoHandleCamera = active;
        }

        /// <summary> Snaps the camera instantly to a scene reference. </summary>
        public void SnapToTransform(Transform target, bool matchFOV = true)
        {
            if (target == null || targetCamera == null)
            {
                return;
            }
            
            if (!_cameraTransform)
            {
                CacheReferences();
            }

            _cameraTransform.SetPositionAndRotation(target.position, target.rotation);
            if (matchFOV && target.TryGetComponent<UnityEngine.Camera>(out var otherCam))
                targetCamera.fieldOfView = otherCam.fieldOfView;

            _targetPose = new CameraPose { position = target.position, rotation = target.rotation, fov = targetCamera.fieldOfView };
            State = CameraState.StaticPose;
        }

        /// <summary> Blends the camera from its current position to a scene reference. </summary>
        public void TransitionToTransform(Transform target, float duration, AnimationCurve curve = null)
        {
            if (target == null) return;

            CameraPose p = new CameraPose
            {
                position = target.position,
                rotation = target.rotation,
                fov = target.TryGetComponent<UnityEngine.Camera>(out var otherCam) ? otherCam.fieldOfView : targetCamera.fieldOfView
            };

            TransitionToPose(p, duration, curve);
        }
 
        /// <summary> Resets to chained rail mode, evaluating all splines. </summary>
        public void ResetToChainMode()
        {
            _activeRailIndex = 0;
        }

        /// <summary> Handles a LookAt switch triggered by a Timeline marker. </summary>
        public void HandleLookAtSwitch(CameraLookAtSwitchMarker m)
        {
            int idx = m.TargetIndex;
            Transform next = (idx >= 0 && idx < lookAtTargets.Count) ? lookAtTargets[idx] : null;
            SetLookAt(next, m.TransitionDuration);
        }

        /// <summary> Immediate LookAt switch with optional duration. </summary>
        public void SetLookAt(Transform target, float duration = 1f)
        {
            if (_lookAtTarget == target)
                return;
            
            _lookAtFrom = _lookAtTarget;
            _lookAtTarget = target;
            _lookAtDuration = duration;
            _lookAtLerp = duration > 0 ? 0f : 1f;
        }

        /// <summary> Generic event handler for legacy markers or external triggers. </summary>
        public void TriggerTimelineEvent(string eventName)
        {
            if (string.IsNullOrEmpty(eventName))
                return;
            
            string lowerEvent = eventName.ToLower();

            if (lowerEvent == "flash")
            {
                targetCamera.DoFlash(Color.white, 0.5f);
            }
            else if (lowerEvent == "wobble")
            {
                targetCamera.DoWobble(5f, 1f, 2f);
            }

            onSplineNotified?.Invoke(eventName);
        }
        #endregion

        
        #region Public Properties
        public float SplineProgress { get => splineProgress; set => splineProgress = Mathf.Clamp01(value); }
        public float LookAtWeight { get => lookAtWeight; set => lookAtWeight = Mathf.Clamp01(value); }

        public UnityEngine.Camera TargetCamera => targetCamera;
        public List<Transform> LookAtTargets => lookAtTargets;
        public Transform CurrentLookAtTarget => _lookAtTarget;
        #endregion

        
        #region INotificationReceiver
        public void OnNotify(Playable origin, INotification notification, object context)
        {
            if (origin.IsValid())
            {
                var director = origin.GetGraph().GetResolver() as PlayableDirector;
                
                if (director != null && playableDirector != director)
                    playableDirector = director;
            }

            if (notification is CameraMarkerBase marker)
            {
                marker.Execute(this);
                OnMarkerEventHit?.Invoke(marker);
            }
        }
        #endregion

#if UNITY_EDITOR
        // ── Editor-only API ──────────────────────────────────────────

        public List<SplineComputer> EditorSplineRails => splineRails;
        public UnityEngine.Camera EditorCamera => targetCamera;
        public PlayableDirector EditorDirector => playableDirector;
        public Transform EditorLookAtTarget => lookAtTarget;
        public List<Transform> EditorLookAtTargets => lookAtTargets;

        public int EditorChaptersCount()
        {
            return chapters.Count;
        }

        public CameraChapter EditorChapter(int index)
        {
            return (index >= 0 && index < chapters.Count) ? chapters[index] : null;
        }

        public int EditorRailCount
        {
            get
            {
                if (splineRails == null)
                {
                    return 0;
                }
                
                int count = 0;
                foreach (var r in splineRails)
                {
                    if (r != null)
                    {
                        count++;
                    }
                }
                return count;
            }
        }

        public int EditorSegmentCountInRail(int railIdx)
        {
            if (splineRails == null || railIdx < 0 || railIdx >= splineRails.Count)
            {
                return 0;
            }

            return splineRails[railIdx] ? Mathf.Max(0, splineRails[railIdx].pointCount - 1) : 0;
        }

        public List<CameraSplineSegment> EditorSegments(int chapterIndex)
        {
            return (chapterIndex >= 0 && chapterIndex < chapters.Count) ? chapters[chapterIndex].segments : new List<CameraSplineSegment>();
        }

        public int EditorTotalSegmentCount
        {
            get
            {
                if (splineRails == null)
                {
                    return 0;
                }
                
                int total = 0;
                foreach (var r in splineRails)
                {
                    if (r)
                    {
                        total += Mathf.Max(0, r.pointCount - 1);
                    }
                }
                return total;
            }
        }

        public SplineSample EditorSampleAt(float progress)
        {
            if (!HasValidRails())
            {
                return new SplineSample();
            }
            
            RebuildChainCache();
            return EvaluateChainAt(Mathf.Clamp01(progress));
        }

        public void EditorEvaluateAt(float progress)
        {
            if (!HasValidRails() || !targetCamera)
            {
                return;
            }
            
            splineProgress = progress;
            RebuildChainCache();
            CameraPose p = SampleRailPose();
            targetCamera.transform.SetPositionAndRotation(p.position, p.rotation);
        }

        public void EditorSyncSegments(int chapterIndex)
        {
            if (splineRails == null || chapterIndex < 0 || chapterIndex >= chapters.Count)
            {
                return;
            }

            var chapter = chapters[chapterIndex];
            var activeSegments = chapter.segments;
            int startRailIdx = chapter.startRailIndex;
            int count = chapter.railCount;
            
            int total = 0;
            int endIdx = Mathf.Min(startRailIdx + count, splineRails.Count);

            for (int i = startRailIdx; i < endIdx; i++)
            {
                if (splineRails[i])
                {
                    total += Mathf.Max(0, splineRails[i].pointCount - 1);
                }
            }

            while (activeSegments.Count > total)
            {
                activeSegments.RemoveAt(activeSegments.Count - 1);
            }
            
            while (activeSegments.Count < total)
            {
                activeSegments.Add(new CameraSplineSegment
                {
                    duration = 1f, 
                    easing = AnimationCurve.EaseInOut(0, 0, 1, 1)
                });
            }
            
            int segIdx = 0;
            for (int r = startRailIdx; r < endIdx; r++)
            {
                if (!splineRails[r])
                {
                    continue;
                }
                
                string prefix = splineRails.Count > 1 ? $"R{r} " : "";
                for (int n = 0; n < splineRails[r].pointCount - 1; n++) 
                {
                    if (segIdx < activeSegments.Count)
                    {
                        activeSegments[segIdx++].label = $"{prefix}Node {n} → {n + 1}";
                    }
                }
            }
        }

        public void AutoCalculateRailCounts()
        {
            if (chapters == null || chapters.Count == 0 || splineRails == null)
            {
                return;
            }

            var sortedChapters = new List<CameraChapter>(chapters);
            sortedChapters.Sort((a, b) => a.startRailIndex.CompareTo(b.startRailIndex));

            for (int i = 0; i < sortedChapters.Count; i++)
            {
                var current = sortedChapters[i];
                
                if (i < sortedChapters.Count - 1)
                {
                    var next = sortedChapters[i + 1];
                    current.railCount = Mathf.Max(1, next.startRailIndex - current.startRailIndex);
                }
                else
                {
                    current.railCount = Mathf.Max(1, splineRails.Count - current.startRailIndex);
                }
            }
        }

        public TimelineAsset EditorGetTimeline(int index)
        {
            return (index >= 0 && index < chapters.Count) ? chapters[index].timeline : null;
        }

        /// <summary> Synchronizes the internal visual state with the Timeline Mixer output. </summary>
        public void EditorReportVisualState(int chapterIdx, float progress)
        {
            if (chapterIdx >= 0 && chapterIdx < chapters.Count)
            {
                _activeChapter = chapters[chapterIdx];
            }
            
            splineProgress = progress;
        }
#endif
    }
}
