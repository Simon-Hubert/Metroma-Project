using System;
using UnityEngine;
using UnityEngine.Events;

namespace Metroma
{
    public class DistanceCounter : MonoBehaviour
    {
        [ConditionParam] private float _distance;
        private Vector3 _lastPos;

        [SerializeField] private UnityEvent _onMoved;

        private void Awake() {
            _lastPos = transform.position;
        }

        void Update() {
            float dist = (_lastPos - transform.position).magnitude;
            _distance += dist;
            if (dist > 0) {
                _onMoved?.Invoke();
            }
            _lastPos = transform.position;
            
        }
    }
}
