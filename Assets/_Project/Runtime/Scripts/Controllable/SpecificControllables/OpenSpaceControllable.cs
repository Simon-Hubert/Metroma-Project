using NaughtyAttributes;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace Metroma
{
    public class OpenSpaceControllable : AControllable
    {
        [SerializeField] private float _accelTime;
        private float _currentAccel;
        [SerializeField] private AnimationCurve _accelCurve;
        
        [SerializeField] private float _speed = 100;
        [ReadOnly, ConditionParam] private float _currentSpeed;
        public float GetSpeed { get => _currentSpeed; }
        
        
        
        protected override void InputMovePerformed(Vector2 move) {
            base.InputMovePerformed(move);

            if (move.x != 0) {
                if (_currentAccel < _accelTime) {
                    _currentAccel += Time.deltaTime;
                    if (_currentAccel > _accelTime) _currentAccel = _accelTime;
                }
            }
            else {
                _currentAccel = 0;
            }

            UpdateSpeed(move.x);
        }

        protected override void InputMoveEnd(Vector2 move) {
            base.InputMoveEnd(move);

            _currentAccel = 0;
            UpdateSpeed(move.x);
        }

        private void UpdateSpeed(float dir)
        {
            _currentSpeed = dir * _speed * _accelCurve.Evaluate(_currentAccel / _accelTime);
        }
    }
}
