using UnityEngine;
using Metroma.CameraTool;
using System.Threading;

namespace Metroma
{
    /// <summary>
    /// Static utility class to bridge the View system with the CameraRig system.
    /// </summary>
    public static class CameraHelpers
    {
        /// <summary>
        /// Transitions a camera to a specific AView.
        /// Uses the active CameraRig to perform the motion.
        /// </summary>
        public static async Awaitable TransitionToViewAsync(Camera cam, AView targetView, float duration, AnimationCurve curve = null)
        {
            if (targetView == null) return;

            CameraRig rig = CameraRig.Active;
            if (rig == null)
            {
                // Fallback: search for rig if Active is not set
                rig = Object.FindFirstObjectByType<CameraRig>();
            }

            if (rig != null)
            {
                CameraConfiguration config = targetView.GetConfiguration();
                CameraPose targetPose = config.ToPose();
                
                rig.TransitionToPose(targetPose, duration, curve);

                // If duration > 0, we might want to wait for the transition to finish
                if (duration > 0f)
                {
                    await Awaitable.FixedUpdateAsync(); // Wait for update loop to start transition
                    float timer = 0f;
                    while (timer < duration)
                    {
                        timer += Time.deltaTime;
                        await Awaitable.NextFrameAsync();
                    }
                }
            }
            else
            {
                Debug.LogWarning("[CameraHelpers] No active CameraRig found to perform TransitionToView.");
            }
        }
    }
}
