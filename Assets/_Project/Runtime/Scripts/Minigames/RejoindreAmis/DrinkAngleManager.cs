using System;
using NaughtyAttributes;
using SMath;
using UnityEngine;

namespace Metroma
{
    public class DrinkAngleManager : MonoBehaviour
    {
        [SerializeField, MinMaxSlider(-20, 20)]
        private Vector2 _velocityRange;

        [SerializeField, MinMaxSlider(-20, 20)]
        private Vector2 _angleRange;

        private Vector3 _lastPos;

        private SecondOrderDynamics<float> _dynamics;

        private void Start() {
            _lastPos = transform.position;
            _dynamics = new SecondOrderDynamics<float>(2, 1, 0, 0, new Linear1D());
        }

        private void FixedUpdate() {
            float vel = (transform.position - _lastPos).magnitude / Time.fixedDeltaTime;
            _lastPos = transform.position;
            float p = Mathf.InverseLerp(_velocityRange.x, _velocityRange.y, vel);
            float angle = Mathf.Lerp(_angleRange.x, _angleRange.y, p);
            angle = _dynamics.Update(Time.fixedDeltaTime, angle);
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }
}
