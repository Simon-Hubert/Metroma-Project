using System;
using NaughtyAttributes;
using UnityEngine;

namespace Metroma
{
    public class AnglePositionner : MonoBehaviour
    {
        [SerializeField] private PlaneControllable _plane;

        [SerializeField, MinMaxSlider(-20, 20)]
        private Vector2 _velocityRange;

        [SerializeField, MinMaxSlider(-20, 20)]
        private Vector2 _angleRange;
        

        private void Update() {
            float p = Mathf.InverseLerp(_velocityRange.x, _velocityRange.y, _plane.vel);
            float angle = Mathf.Lerp(_angleRange.x, _angleRange.y, p);
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }
}   
