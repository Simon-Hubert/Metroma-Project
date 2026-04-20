using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using System.Collections.Generic;

namespace Metroma.CameraTool
{
    /// <summary>
    /// Defines a static camera view with position, rotation, FOV and distance offset.
    /// Optimized for memory alignment and fast interpolation.
    /// </summary>
    [System.Serializable]
    public struct CameraPose
    {
        public Vector3 position;
        public Quaternion rotation;
        public float fov;
        public float distance;
        public Vector3 up;
        public int railIdx;
        public float progressInsideRail;

        public static CameraPose Identity => new CameraPose
        {
            position = Vector3.zero,
            rotation = Quaternion.identity,
            fov = 60f,
            distance = 0f,
            up = Vector3.up,
            railIdx = 0,
            progressInsideRail = 0f
        };

        public static CameraPose Lerp(CameraPose InA, CameraPose InB, float InT)
        {
            return new CameraPose
            {
                position = Vector3.LerpUnclamped(InA.position, InB.position, InT),
                rotation = Quaternion.SlerpUnclamped(InA.rotation, InB.rotation, InT),
                fov = Mathf.LerpUnclamped(InA.fov, InB.fov, InT),
                distance = Mathf.LerpUnclamped(InA.distance, InB.distance, InT),
                up = Vector3.SlerpUnclamped(InA.up, InB.up, InT),
                railIdx = InT < 0.5f ? InA.railIdx : InB.railIdx,
                progressInsideRail = Mathf.LerpUnclamped(InA.progressInsideRail, InB.progressInsideRail, InT)
            };
        }
    }

    /// <summary>
    /// Defines a cinematic chapter with its own Timeline and pacing.
    /// Component of the modular SequenceModule.
    /// </summary>
    [System.Serializable]
    public class CameraChapter
    {
        public string name;
        public TimelineAsset timeline;
        
        [Tooltip("The rail index where this chapter starts its journey.")]
        public int startRailIndex = 0;

        [Tooltip("Number of rails this chapter controls starting from the Start Rail Index.")]
        public int railCount = 1;

        [Tooltip("Initial wait time in seconds before the camera starts moving.")]
        public float waitAtStart = 0f;
        
        [Tooltip("Custom color for this chapter's gizmos.")]
        public Color debugColor = Color.cyan;

        [HideInInspector] public bool isExpanded = true;

        /// <summary> List of pacing segments specific to this chapter. </summary>
        public List<CameraSplineSegment> segments = new List<CameraSplineSegment>();

        [Header("Pacing Engine")]
        public CameraPacingMode pacingMode = CameraPacingMode.Segmented;
        public AnimationCurve globalCurve = AnimationCurve.Linear(0, 0, 1, 1);

        #region --- Runtime Cache ---

        [System.NonSerialized] public float cachedTotalMoveDuration = -1f;
        [System.NonSerialized] public float[] cachedRailDurations;
        [System.NonSerialized] public int[] cachedRailSegmentStarts;
        [System.NonSerialized] public float cachedPhysicalLength = -1f;

        #endregion
    }

    /// <summary> Defines how the camera progress is evaluated across the chapter. </summary>
    public enum CameraPacingMode
    {
        Segmented,     // Individual curves per segment (Legacy)
        Global,        // One curve for the whole chapter
        ConstantSpeed  // Uniform speed, ignores individual segment durations
    }

    /// <summary> Current state of the camera controller. </summary>
    public enum CameraState
    {
        FollowRail,      // Normal movement along spline
        Transitioning,   // Blending from current to a StaticPose
        StaticPose,      // Holding on a static position
        ReturningToRail  // Blending from a StaticPose back to spline
    }

    /// <summary> Action to perform on the Timeline director when reaching a transition end. </summary>
    public enum DirectorAction
    {
        None,
        Pause,
        Stop
    }

    /// <summary> Local override settings for a rail junction. </summary>
    [System.Serializable]
    public struct JunctionSettings
    {
        [Tooltip("Timeline ONLY: Duration in seconds to overlap/blend between this rail and the next one.")]
        public float duration;

        [Tooltip("Distance (in meters) to blend between rails when in manual/non-timeline mode.")]
        public float blendDistance;

        [Tooltip("How lazy/smooth the transition is. Higher = softer cinematic movement (e.g., 5-8). 1 = Linear.")]
        [Range(1f, 10f)]
        public float smoothness;

        public static JunctionSettings Default => new JunctionSettings { duration = 1.0f, blendDistance = 1.0f, smoothness = 5.0f };
    }

    /// <summary> Target for camera rotation during a chapter transition. </summary>
    public enum TransitionLookAtMode
    {
        TimelineDefault,
        ChapterStart,
        ChapterEnd,
        SpecificGameObject
    }
}
