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
            Quaternion rot = GetRotation();
            return new Metroma.CameraTool.CameraPose
            {
                position = Pivot + (rot * Vector3.forward * -Distance),
                rotation = rot,
                fov = Fov,
                distance = Distance,
                up = rot * Vector3.up
            };
        }

        public Quaternion GetRotation() => Quaternion.Euler(Pitch, Yaw, Roll);
        public Vector3 GetPosition() => Pivot + (GetRotation() * Vector3.forward * -Distance);

        public static CameraConfiguration operator +(CameraConfiguration a, CameraConfiguration b)
        {
            return new CameraConfiguration
            {
                Pivot = a.Pivot + b.Pivot,
                Distance = a.Distance + b.Distance,
                Yaw = a.Yaw + b.Yaw,
                Pitch = a.Pitch + b.Pitch,
                Roll = a.Roll + b.Roll,
                Fov = a.Fov + b.Fov
            };
        }

        public static CameraConfiguration operator *(CameraConfiguration a, float b)
        {
            return new CameraConfiguration
            {
                Pivot = a.Pivot * b,
                Distance = a.Distance * b,
                Yaw = a.Yaw * b,
                Pitch = a.Pitch * b,
                Roll = a.Roll * b,
                Fov = a.Fov * b
            };
        }

        public static CameraConfiguration operator *(float a, CameraConfiguration b) => b * a;
    }

    /// <summary>
    /// Abstract base class for all Camera Views.
    /// </summary>
    public abstract class AView : MonoBehaviour
    {
        public abstract CameraConfiguration GetConfiguration();
    }
}
