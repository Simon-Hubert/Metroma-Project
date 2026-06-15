using System;
using UnityEngine;

namespace Metroma
{
    public class FakeBuoyancy : MonoBehaviour
    {
        [SerializeField] private float _amplitude;
        [SerializeField] private float _frequency;
        private Vector3 _origin;

        private void Start() {
            _origin = transform.localPosition;
        }

        private void Update() {
            transform.localPosition = _origin + _amplitude * Mathf.Sin(2*Mathf.PI*_frequency*Time.time) * Vector3.up;
        }
    }
}
