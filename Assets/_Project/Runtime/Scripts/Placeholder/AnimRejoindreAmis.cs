using System;
using SMath;
using UnityEngine;

namespace Metroma
{
    public class AnimRejoindreAmis : MonoBehaviour
    {
        [SerializeField] private Transform _toMove;
        [SerializeField] private Transform _from;
        [SerializeField] private Transform _to;
        [SerializeField] private int _max;
        [SerializeField] private AnimationCurve _pos;
        [SerializeField] private float _f, _z, _r;
        private Vector3 _targetPos;
        private SecondOrderDynamics<Vector3> _dynamics;

        private void Awake() {
            _targetPos = _from.position;
        }

        private void Start() {
            _dynamics = new SecondOrderDynamics<Vector3>(_f, _z, _r, _from.position, new Linear3D());
        }

        public void UpdateTargetPos(int value) {
            _targetPos = Vector3.Lerp(_from.position, _to.position, _pos.Evaluate((float)value / _max));
        }
        
        private void Update() {
            if (Time.deltaTime > 0f)
            {
                _toMove.position = _dynamics.Update(Time.deltaTime, _targetPos);
            }
        }

        private void OnDrawGizmos() {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(_from.position, _to.position);
            for (int i = 0; i <= _max; i++) {
                Gizmos.DrawSphere(Vector3.Lerp(_from.position, _to.position, _pos.Evaluate((float)i / _max)), 0.1f);
            }
        }
    }
}
