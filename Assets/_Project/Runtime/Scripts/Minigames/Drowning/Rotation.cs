using System;
using UnityEngine;

namespace Metroma
{
    public class Rotation : MonoBehaviour
    {
        [SerializeField] private float _rotationSpeed;
        [SerializeField] private bool _clockwise;
        private float _direction;

        private void Start() {
            _direction = _clockwise ? -1 : 1;
        }

        private void Update() {
            Quaternion rotation = transform.localRotation;
            rotation.eulerAngles = rotation.eulerAngles + new Vector3(0, 0,_direction * _rotationSpeed * Time.deltaTime);
            transform.localRotation = rotation;
        }
    }
}
