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
        
        public event Action<Direction> OnChooseDirection;
        
        protected override void InputMovePerformed(Vector2 move) {
            base.InputMovePerformed(move);

            if (Mathf.Abs(move.x) > _deadZone) {
                if (move.x > 0 && _direction != Direction.RIGHT) {
                    _direction = Direction.RIGHT;
                    OnChooseDirection?.Invoke(_direction);
                }
                else if (move.x < 0 && _direction != Direction.LEFT) {
                    _direction = Direction.LEFT;
                    OnChooseDirection?.Invoke(_direction);
                }
            }
            else {
                _direction = Direction.NONE;
            }
        }

        protected override void InputMoveEnd(Vector2 move)
        {
            base.InputMoveEnd(move);
            
            _direction = Direction.NONE;
        }
    }
}
