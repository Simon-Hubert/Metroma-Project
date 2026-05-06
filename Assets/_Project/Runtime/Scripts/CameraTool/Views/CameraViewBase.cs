using UnityEngine;

namespace Metroma
{
    /// <summary>
    /// Base configuration for camera views.
    /// Used by the View system to define targets.
    /// </summary>
    [System.Serializable]
    public struct CameraConfiguration
    {
        public Vector3 Pivot;
        public float Distance;
        public float Yaw;
        public float Pitch;
        public float Roll;
        public float Fov;

        /// <summary>
        /// Converts this configuration to the standard Metroma.CameraTool.CameraPose.
        /// </summary>
        public Metroma.CameraTool.CameraPose ToPose()
        {
            Quaternion rot = Quaternion.Euler(Pitch, Yaw, Roll);
            return new Metroma.CameraTool.CameraPose
            {
                position = Pivot + (rot * Vector3.forward * -Distance),
                rotation = rot,
                fov = Fov,
                distance = Distance,
                up = rot * Vector3.up
            };
        }
    }

    /// <summary>
    /// Abstract base class for all Camera Views.
    /// </summary>
    public abstract class AView : MonoBehaviour
    {
        public abstract CameraConfiguration GetConfiguration();
    }
}
