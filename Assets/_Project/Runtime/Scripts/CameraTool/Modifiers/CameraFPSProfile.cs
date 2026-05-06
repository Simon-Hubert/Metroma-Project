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

        [Header("Visual & Juice")]
        [Tooltip("FOV override during FPS mode. Set to -1 to keep the current FOV.")]
        public float fovOverride = -1f;

        [Tooltip("Amount of dynamic roll (tilt) when looking sideways.")]
        [Range(0f, 5f)]
        public float tiltAmount = 1.0f;

        [Tooltip("How fast the camera returns to horizontal after tilting.")]
        [Range(0.1f, 10f)]
        public float tiltReturnSpeed = 5.0f;

        [Tooltip("Adds weight and momentum to rotation. High value = Heavy camera.")]
        [Range(0f, 1f)]
        public float rotationMomentum = 0.5f;

        [Header("Procedural Motion")]
        [Tooltip("Enable or disable the breathing/sway effect.")]
        public bool useSway = true;
        
        [Tooltip("Enable or disable the high-frequency micro-jitter effect.")]
        public bool useJitter = true;

        [Tooltip("Enable or disable the dynamic roll/tilt when looking sideways.")]
        public bool useTilt = true;

        [Tooltip("Optional Handheld Profile to drive the camera motion. If assigned, legacy settings below are ignored (but toggles above still apply to the profile's influence).")]
        public CameraHandheldProfile handheldProfile;

        [Header("Legacy Motion (Manual)")]
        [Tooltip("Amount of procedural handheld sway (breath) per axis (X=Horiz, Y=Vert).")]
        public Vector2 swayAmount = new Vector2(0.1f, 0.2f);

        [Tooltip("Speed of the handheld sway.")]
        [Range(0f, 5f)]
        public float swaySpeed = 1.0f;

        [Tooltip("Amount of high-frequency micro-jitter (shaky hands).")]
        [Range(0f, 5f)]
        public float jitterAmount = 0.1f;

        [Tooltip("Speed of the micro-jitter.")]
        [Range(0.1f, 50f)]
        public float jitterSpeed = 10.0f;
        
        public bool IsYawUnlimited => yawLimits.x <= -179f && yawLimits.y >= 179f;
    }
}
