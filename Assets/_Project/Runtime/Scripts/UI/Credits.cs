using System;
using UnityEngine;

namespace Metroma
{
    public class Credits : MonoBehaviour
    {
        [SerializeField] private float _speed;
        private bool _started;
        private RectTransform _transform;
        
        private void Update() {
            if (!_started) return;
            transform.localPosition += Vector3.up * (_speed * Time.deltaTime);
        }

        public void Play() {
            _started = true;
            _transform = GetComponent<RectTransform>();
        }
    }
}
