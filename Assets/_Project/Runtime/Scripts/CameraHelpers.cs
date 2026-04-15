using System;
using System.Collections;
using System.Threading;
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
            return from * (1-p) + p * to;
        }

        public static void TransitionViewToView(Camera cam, AView from, AView to, float duration) {
            _ = ViewToViewTransitionAsync(cam, from, to, duration);
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
