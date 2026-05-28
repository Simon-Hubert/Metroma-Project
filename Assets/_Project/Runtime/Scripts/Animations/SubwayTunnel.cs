using System;
using Dreamteck.Splines;
using NaughtyAttributes;
using UnityEngine;

namespace Metroma
{
    public class SubwayTunnel : MonoBehaviour
    {
        [SerializeField] private float _partsDistance = 7.5f;
        [SerializeField, ReadOnly] private float _progression = 0.5f;
        [SerializeField] public float speed = 1;

        [SerializeField, ReadOnly] private Vector3 _direction = Vector3.right;
        [SerializeField, ReadOnly] private Vector3 _initPos;

        [SerializeField, ReadOnly] private bool _isActive;
        public void ActiveLoop(bool active) => _isActive = active;
        public void ActiveLoopWithInit(bool active) {
            _isActive = active;
            Init();
        }

        private void Start() {
            Init();
        }

        private void FixedUpdate()
        {
            if (!_isActive) return;
            
            _progression = Mathf.Repeat(_progression + (speed * Time.fixedDeltaTime / _partsDistance), 1.0f);
            SetPos(_progression);
        }


        private void Init() {
            _initPos = transform.position;
            _direction = transform.right;
            _progression = 0.5f;
            SetPos(_progression);
        }
        private void SetPos(float progression) {
            progression -= 0.5f;
            transform.position = _initPos + (progression * _partsDistance) * _direction;
        }
    }
}
