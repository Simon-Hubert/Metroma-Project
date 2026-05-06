using System;
using UnityEngine;
using Metroma.Utils;
using NaughtyAttributes;

namespace Metroma
{
    public class TrolleyControllable : AControllable
    {
        [SerializeField, Range(0, 1)] private float _deadZone;
        [SerializeField, ReadOnly] private Direction _direction;

        public event Action OnChooseLeft;
        public event Action OnChooseRight;
        
        protected override void InputMovePerformed(Vector2 move)
        {
            base.InputMovePerformed(move);

            if (Mathf.Abs(move.x) > _deadZone) {
                if (move.x > 0 && _direction != Direction.RIGHT) {
                    OnChooseRight?.Invoke();
                    _direction = Direction.RIGHT;
                }
                else if (_direction != Direction.LEFT) {
                    OnChooseLeft?.Invoke();
                    _direction = Direction.LEFT;
                }
            }
            else {
                _direction = Direction.NONE;
            }
        }
    }
}
