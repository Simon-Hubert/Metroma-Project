using UnityEngine;

namespace Metroma.CameraTool.Modifiers
{
    [CreateAssetMenu(fileName = "NewShakeProfile", menuName = "Camera/Profiles/Shake")]
    public class CameraShakeProfile : ScriptableObject
    {
        public float intensity = 0.5f;
        public float roughness = 1.0f;
        public bool fadeOut = true;

        [Tooltip("Optional curve to scale intensity over time. If null, intensity remains constant.")]
        public AnimationCurve intensityCurve = AnimationCurve.Constant(0, 1, 1);

        [Tooltip("If true, this shake will also trigger gamepad haptics.")]
        public bool syncHaptics = true;
    }
}
