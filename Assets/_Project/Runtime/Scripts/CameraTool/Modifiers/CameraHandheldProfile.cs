using UnityEngine;

namespace Metroma.CameraTool.Modifiers
{
    /// <summary>
    /// Configuration profile for handheld camera motion.
    /// Real-world handheld motion affects both position and rotation.
    /// </summary>
    [CreateAssetMenu(fileName = "NewHandheldProfile", menuName = "Camera/Profiles/Handheld")]
    public class CameraHandheldProfile : ScriptableObject
    {
        [Header("Intensity")]
        [Tooltip("Position noise intensity per axis.")]
        public Vector3 positionIntensity = new Vector3(0.01f, 0.01f, 0.01f);
        
        [Tooltip("Rotation noise intensity per axis (Euler).")]
        public Vector3 rotationIntensity = new Vector3(0.5f, 0.5f, 0.1f);

        [Header("Dynamics")]
        [Tooltip("How fast the noise evolves. Higher = more jittery.")]
        public float roughness = 1.0f;

        [Tooltip("Physical weight/inertia of the camera. Higher = smoother/heavier feel.")]
        [Range(0f, 1f)]
        public float damping = 0.2f;

        [Tooltip("Scale for the overall effect.")]
        [Range(0f, 2f)]
        public float influence = 1.0f;

        [Header("Breathing (Rhythmic)")]
        [Tooltip("Optional curve to scale the handheld effect over time (looping).")]
        public AnimationCurve breathCurve = AnimationCurve.Constant(0, 1, 1);
        
        [Tooltip("How fast the breath curve loops (Hz).")]
        public float breathSpeed = 0.5f;
    }
}
