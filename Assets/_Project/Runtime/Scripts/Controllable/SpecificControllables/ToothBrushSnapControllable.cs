using System;
using System.Collections.Generic;
using System.Threading;
using SMath;
using UnityEngine;

namespace Metroma
{
    public class ToothBrushSnapControllable : AControllable
    {
        [SerializeField] private float _distance;
        [SerializeField] private AnimationCurve _distanceOverAngle;
        private const float _resetDuration = 0.1f;

        [SerializeField] private Transform _center;
        
        private Vector3 _targetOffset;
        private Awaitable _inputReset;

        private SecondOrderDynamics<Vector3> _dynamics;
        
        protected override void Start() {
            base.Start();
            _dynamics = new SecondOrderDynamics<Vector3>(0.5f, 1, 1, _center.position, new Linear3D());
        }

        protected override void Update() {
            base.Update();
            if (!IsActive) return;
            Vector3 target = _center.position + _targetOffset;
            transform.position = _dynamics.Update(Time.deltaTime, target);
        }

        protected override void InputMoveStart(Vector2 move) {
            base.InputMoveStart(move);
            if (_inputReset != null) {
                _inputReset.Cancel();
                _inputReset = null;
            }

            float horizontality = Mathf.Abs(Vector2.Dot(move.normalized, Vector2.left));
            
            _targetOffset = move.normalized * _distance * _distanceOverAngle.Evaluate(horizontality);
            _inputReset = InputStarted();
        }

        private async Awaitable InputStarted() {
            try {
                await Awaitable.WaitForSecondsAsync(_resetDuration);
            }
            catch (OperationCanceledException oce) { }
            finally {
                _targetOffset = Vector3.zero;
            }
        }

        private void OnDrawGizmosSelected() {
            List<Vector3> points = new List<Vector3>();
            points.Add(_center.position - _distance * Vector3.left);
            for (float i = 0; i < 360; i += 10) {
                float angle = i * Mathf.Deg2Rad;
                Vector3 target = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0);
                float horizontality = Mathf.Abs(Vector2.Dot(target, Vector2.left));
                points.Add(_center.position + target * _distance * _distanceOverAngle.Evaluate(horizontality));
                points.Add(_center.position + target * _distance * _distanceOverAngle.Evaluate(horizontality));
            }
            points.Add(_center.position - _distance * Vector3.left);
            
            
            Gizmos.DrawLineList(points.ToArray());
        }
    }
}
