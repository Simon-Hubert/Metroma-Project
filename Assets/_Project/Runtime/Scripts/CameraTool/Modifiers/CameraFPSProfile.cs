using UnityEngine;
using UnityEngine.InputSystem;

namespace Metroma.CameraTool.Modifiers
{
    /// <summary>
    /// Configuration profile for FPS observation mode.
    /// Defines input, sensitivity, rotation constraints, and visual overrides.
    /// </summary>
    [CreateAssetMenu(fileName = "NewFPSProfile", menuName = "Camera/Profiles/FPS Observation")]
    public class CameraFPSProfile : ScriptableObject
    {
        [Header("Input")]
        [Tooltip("InputAction reference for the Look axis (Mouse Delta or Right Stick). If null, falls back to Mouse.current.delta.")]
        public InputActionReference lookAction;

        [Header("Sensitivity")]
        [Tooltip("Sensitivity multiplier per axis (X = Horizontal, Y = Vertical).")]
        public Vector2 sensitivity = new Vector2(2f, 2f);

        [Tooltip("Invert the vertical (pitch) axis.")]
        public bool invertY = false;

        [Header("Rotation Limits")]
        [Tooltip("Vertical rotation limits in degrees (X = min, Y = max).")]
        public Vector2 pitchLimits = new Vector2(-80f, 80f);

        [Tooltip("Horizontal rotation limits in degrees (X = min, Y = max). Use (-180, 180) for unlimited rotation.")]
        public Vector2 yawLimits = new Vector2(-180f, 180f);

        [Header("Smoothing")]
        [Tooltip("Input smoothing factor. 0 = raw/instant, higher = smoother/laggier. Recommended: 5-15 for cinematic, 0 for responsive.")]
        [Range(0f, 30f)]
        public float inputSmoothing = 0f;

        [Header("Visual")]
        [Tooltip("FOV override during FPS mode. Set to -1 to keep the current FOV.")]
        public float fovOverride = -1f;

        public bool IsYawUnlimited => yawLimits.x <= -179f && yawLimits.y >= 179f;
    }
}
