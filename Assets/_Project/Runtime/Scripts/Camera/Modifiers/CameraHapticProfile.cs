using UnityEngine;

namespace Metroma.CameraTool.Modifiers
{
    /// <summary>
    /// Configuration profile for dedicated gamepad haptics.
    /// </summary>
    [CreateAssetMenu(fileName = "NewHapticProfile", menuName = "Camera/Profiles/Haptic")]
    public class CameraHapticProfile : ScriptableObject
    {
        [Tooltip("Base intensity curve over the duration (0.0 to 1.0).")]
        public AnimationCurve intensityCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.2f, 1), new Keyframe(1, 0));

        [Tooltip("Scale for the low frequency motor (Large rumble).")]
        [Range(0f, 2f)]
        public float lowFreqMultiplier = 1.0f;

        [Tooltip("Scale for the high frequency motor (Small jitter).")]
        [Range(0f, 2f)]
        public float highFreqMultiplier = 1.0f;

        [Tooltip("Duration of the haptic effect in seconds.")]
        [Min(0.01f)]
        public float duration = 0.5f;
    }
}
