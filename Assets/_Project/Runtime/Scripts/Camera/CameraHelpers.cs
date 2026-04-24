using System;
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

        public static CameraConfiguration Lerp(CameraConfiguration from, CameraConfiguration to, float p) {
            float toYaw = from.Yaw + Mathf.DeltaAngle(from.Yaw, to.Yaw);
            CameraConfiguration result = from * (1 - p) + p * to;
            toYaw = from.Yaw * (1 - p) + p * toYaw;
            result.Yaw = toYaw;
            return result;
        }

        public static void TransitionViewToView(Camera cam, AView from, AView to, float duration) {
            _ = ViewToViewTransitionAsync(cam, from, to, duration);
        }

        public static async Awaitable TransitionToViewAsync(Camera cam, AView to, float duration) {
            CameraConfiguration toConfig = to.GetConfiguration();
            toConfig.Yaw = Mathf.DeltaAngle(0, toConfig.Yaw);
            CameraConfiguration fromConfig = new CameraConfiguration()
            {
                Distance = 0,
                Fov = cam.fieldOfView,
                Pitch = Mathf.DeltaAngle(0,cam.transform.rotation.eulerAngles.x),
                Yaw = Mathf.DeltaAngle(0,cam.transform.rotation.eulerAngles.y),
                Roll = Mathf.DeltaAngle(0,cam.transform.rotation.eulerAngles.z),
                Pivot = cam.transform.position,
            };
            ApplyConfiguration(cam, fromConfig);
            float t = 0;
            while (t < duration) {
                t += Time.deltaTime;
                float p = t / duration;
                ApplyConfiguration(cam, Lerp(fromConfig, toConfig, p));
                await Awaitable.NextFrameAsync();
            }
            ApplyConfiguration(cam, toConfig);
        }

        public static async Awaitable ViewToViewTransitionAsync(Camera cam, AView from, AView to, float duration) {
            try {
                CameraConfiguration fromConfig = from.GetConfiguration();
                CameraConfiguration toConfig = to.GetConfiguration();
                ApplyConfiguration(cam, fromConfig);
                float t = 0;
                while (t < duration) {
                    t += Time.deltaTime;
                    float p = t / duration;
                    ApplyConfiguration(cam, Lerp(fromConfig, toConfig, p));
                    await Awaitable.NextFrameAsync();
                }
                ApplyConfiguration(cam, toConfig);
            }
            catch (OperationCanceledException oce) {

            }
            finally {
                
            }
        }
    }
}
