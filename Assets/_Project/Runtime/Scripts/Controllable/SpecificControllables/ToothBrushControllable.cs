using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Metroma
{
    public class ToothBrush : AControllable
    {
        [Header("Parametres Constants")]
        [SerializeField] private float _maxSpeed;
        [SerializeField] private float _maxAccel;
        
        [Header("References")]
        [SerializeField] private Rigidbody2D _rb2D;
        private Vector2 _moveDirection;

        private Vector2 _velocity;

        private Coroutine movementRoutine;


        protected override void FixedUpdate() {
            // Error Proof
            if (!_rb2D) {
                Debug.LogError($"{name} : Missing RigidBody2D");
                return;
            }

            Vector2 desiredVelocity = _moveDirection * _maxSpeed;
            float maxSpeedChange = _maxAccel * Time.deltaTime;
            _velocity.x = Mathf.MoveTowards(_velocity.x, desiredVelocity.x, maxSpeedChange);
            _velocity.y = Mathf.MoveTowards(_velocity.y, desiredVelocity.y, maxSpeedChange);

            _rb2D.linearVelocity = _velocity;
        }

        
        protected override void InputMoveStart(Vector2 move) {
            base.InputMoveStart(move);
            movementRoutine ??= StartCoroutine(MoveRoutine());
        }
        
        protected override void InputMoveEnd(Vector2 move) {
            base.InputMoveEnd(move);
            if (movementRoutine != null) {
                StopCoroutine(movementRoutine);
            }
            movementRoutine = null;
            _moveDirection = Vector2.zero;
        }   
        
        IEnumerator MoveRoutine() {
            while (true) {
                yield return null;
                _moveDirection = Inputs.moveDir;
            }
        }
    }
}
