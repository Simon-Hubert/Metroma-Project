using System.Collections;
using System.Collections.Generic;
using Dreamteck.Splines;
using UnityEngine;
using static UnityEngine.Mathf;

namespace Metroma
{
    public class DollyView : AView
    {
        [SerializeField] private float _roll;
        [SerializeField] private float _distance;
        [SerializeField] private float _fov;
        [SerializeField] private SplineComputer _rail;
        [SerializeField] private float _distanceOnRail;
        [SerializeField] private float _speed;
        [SerializeField] private AnimationCurve _curve;
    
        private float _yaw;
    
        public override CameraConfiguration GetConfiguration() {
            float p = Clamp(_distanceOnRail, 0, 1);
            Vector3 dir = _rail.Evaluate(_distanceOnRail).forward;
            dir.Normalize();

            _yaw += DeltaAngle(_yaw, Atan2(dir.x, dir.z) * Rad2Deg);
        
            return new CameraConfiguration
            {
                Yaw = _yaw,
                Pitch = -Asin(dir.y) * Rad2Deg,
                Roll = _roll,
                Fov = _fov,
                Pivot = _rail.EvaluatePosition(_distanceOnRail),
                Distance = _distance
            };
        }

        public async Awaitable AnimateView(float duration) {
            float t = 0;
            _distanceOnRail = 0;
            while (t < duration) {
                await Awaitable.EndOfFrameAsync();
                t += Time.deltaTime;
                float p = t / duration;
                p = Clamp(p, 0, 1);
                p = _curve.Evaluate(p);
                _distanceOnRail = p;
            }
            _distanceOnRail = 1;
        }
    }
}

