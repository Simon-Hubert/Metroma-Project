using System;
using System.ComponentModel;
using UnityEngine;
using Metroma.CameraTool.Modifiers;


namespace Metroma.CameraTool.Timeline
{
    /// <summary>
    /// Timeline marker: triggers a standalone gamepad haptic effect.
    /// </summary>
    [Serializable]
    [DisplayName("Camera/🎮 Gamepad Haptic")]
    public class CameraHapticMarker : CameraMarkerBase
    {
        [Tooltip("Optional preset profile. If assigned, settings below are ignored.")]
        [SerializeField] private CameraHapticProfile profile;

        [Header("Direct Settings")]
        [Tooltip("Haptic intensity curve over the duration.")]
        [SerializeField] private AnimationCurve intensityCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.2f, 1), new Keyframe(1, 0));

        [Tooltip("Low frequency motor intensity (Large rumble).")]
        [Range(0f, 2f)]
        [SerializeField] private float lowFreq = 1.0f;

        [Tooltip("High frequency motor intensity (Small jitter).")]
        [Range(0f, 2f)]
        [SerializeField] private float highFreq = 1.0f;

        [Tooltip("Duration of the haptic effect.")]
        [Min(0.01f)]
        [SerializeField] private float duration = 0.5f;

        [Header("Manual Patterns")]
        [SerializeField] private bool usePattern = false;
        [Range(1, 4)]
        [SerializeField] private int pulseCount = 1;
        [Range(0.05f, 0.5f)]
        [SerializeField] private float pulseInterval = 0.15f;

        public override void Execute(CameraTool tool)
        {
            if (tool == null || tool.TargetCamera == null)
                return;

            if (profile != null)
            {
                CameraModifiers.DoHaptic(tool.TargetCamera, profile);
            }
            else
            {
                float finalDuration = usePattern ? (pulseInterval * (pulseCount + 1)) : duration;
                CameraModifiers.DoHaptic(tool.TargetCamera, intensityCurve, lowFreq, highFreq, finalDuration, usePattern, pulseCount, pulseInterval);
            }
        }
    }
}
