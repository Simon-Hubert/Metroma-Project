using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Metroma
{
    public class ScratchControllable : Controllable
    {
        [SerializeField] private InputManager _inputManager;
        [SerializeField] private float _distance;
        [SerializeField] private float _speed;
        private float _travelled;
        private float _totalDistance;
        public float TotalDistance => _totalDistance;
        [SerializeField] private Vector3 _origin;
        private Coroutine _moveRoutine;
        private bool _init;

        [SerializeField] private UnityEvent _onScratch;
        
        private void OnEnable() {
            OnMoveStart += MoveStarted;
            OnMoveEnd += MoveEnded;
            _inputManager.AddControllable(this);
        }

        private void OnDisable() {
            OnMoveStart -= MoveStarted;
            OnMoveEnd -= MoveEnded;
        }

        private void MoveStarted() {
            if (!_init) {
                _origin = transform.position;
                _init = true;
            }
            _moveRoutine = StartCoroutine(MoveRoutine());
        }
        
        private void MoveEnded() {
            StopCoroutine(_moveRoutine);
        }

        private IEnumerator MoveRoutine() {
            while (true) {
                yield return null;
                float dir = Mathf.Sign(Vector2.Dot(Inputs.move, Vector2.left));
                float lastTravelled = _travelled;
                _travelled = Mathf.Clamp(_travelled + _speed * Time.deltaTime * dir, -_distance, _distance);
                float dist = Mathf.Abs(_travelled - lastTravelled);
                _totalDistance += dist;
                if (dist > Mathf.Epsilon) _onScratch?.Invoke();
                
                
                transform.position = _origin + _travelled * Vector3.left;
            }
        }

        private void OnDrawGizmos() {
            Gizmos.color = Color.chartreuse;
            Gizmos.DrawLine(transform.position, transform.position + _distance*Vector3.left);
            Gizmos.DrawSphere(transform.position + _distance*Vector3.left, 0.25f);
            Gizmos.DrawLine(transform.position, transform.position - _distance*Vector3.left);
            Gizmos.DrawSphere(transform.position - _distance*Vector3.left, 0.25f);
            
        }
    }
}
