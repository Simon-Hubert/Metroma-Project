using System.ComponentModel;
using Metroma.CameraTool;
using UnityEngine.Timeline;
using System;
using UnityEngine;
using UnityEngine.Playables;

namespace Metroma.CameraTool.Timeline
{
    /// <summary>
    /// Runtime data for a single CameraRig clip on the Timeline.
    /// Holds the evaluation parameters and easing for a specific rail or chapter segment.
    /// </summary>
    [Serializable]
    public class CameraToolBehaviour : PlayableBehaviour
    {
        [Tooltip("If >= 0, progress (0-1) is mapped to this specific rail index. If -1, it maps manually.")]
        public int railIndex = -1;

        [Tooltip("The chapter context for this clip.")]
        public int chapterIndex = -1;

        [Range(0f, 1f)]
        public float startProgress;

        [Range(0f, 1f)]
        public float endProgress = 1f;

        public AnimationCurve easingCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        [Range(0f, 1f)]
        [Tooltip("Blend between spline rotation (0) and LookAt target (1) for this clip.")]
        public float lookAtWeight = 1f;

        [HideInInspector]
        public double clipStartTime;
        [HideInInspector]
        public double clipDuration;
    }
}
