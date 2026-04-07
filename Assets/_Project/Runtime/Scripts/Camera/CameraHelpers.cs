using UnityEngine;

namespace Metroma
{
    public static class CameraHelpers
    {
        public static void ApplyConfiguration(Camera cam, CameraConfiguration config) {
            cam.transform.rotation = config.GetRotation();
            cam.transform.position = config.GetPosition();
            cam.fieldOfView = config.Fov;
        }
    }
}
