using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Metroma
{
    public class DrowningControllable : Controllable
    {
        [Header("Parametres changeant (par Pub)")]
        [SerializeField] private float _lifeTime;
        
        [Header("Parametres Constants")]
        [SerializeField] private float _maxSpeed;
        [SerializeField] private float _maxAccel;
        [SerializeField] private float _dashDistance;
        [SerializeField] private float _dashFriction;
        [SerializeField] private float _additionalForceFriction;
        
        [Header("References")]
        [SerializeField] private Rigidbody2D _rb2D;
        [SerializeField] private Transform _respawnPos;

        [SerializeField] private UnityEvent _onDeath;
        
        private Vector2 _additionalForce;
        private Vector2 _dashForce;
        private Vector2 _moveDirection;

        private Vector2 _velocity;

        private Coroutine dashRoutine;
        private Coroutine movementRoutine;

        protected override void Start() {
            base.Start();
            Respawn();
        }

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

            _rb2D.linearVelocity = _velocity + _additionalForce + _dashForce;

            _additionalForce -= _additionalForceFriction * _additionalForce;
        }

        public void Respawn() {
            transform.position = _respawnPos.position;
            StartCoroutine(LifeRoutine());
        }
        
        public void SetAdditionalForce(Vector2 force) {
            _additionalForce = force;
        }

        protected override void InputMoveStart() {
            base.InputMoveStart();
            movementRoutine ??= StartCoroutine(MoveRoutine());
        }
        
        protected override void InputMoveEnd() {
            base.InputMoveEnd();
            if (movementRoutine != null) {
                StopCoroutine(movementRoutine);
            }
            movementRoutine = null;
            _moveDirection = Vector2.zero;
        }   

        protected override void InputActionStart() {
            base.InputActionStart();
            Debug.Log("Dashed");
            _dashForce += _moveDirection * _dashDistance;
            Vector2.ClampMagnitude(_dashForce, _dashDistance);
            if (dashRoutine == null) {
                dashRoutine = StartCoroutine(DashCoroutine());
            }
        }
        

        IEnumerator DashCoroutine() {
            while (true) {
                yield return new WaitForFixedUpdate();
                _dashForce -= _dashFriction * _dashForce * Time.deltaTime;
            }
        }
        
        IEnumerator MoveRoutine() {
            while (true) {
                yield return null;
                _moveDirection = Inputs.moveDir;
            }
        }

        IEnumerator LifeRoutine() {
            float t = 0;
            while (t < _lifeTime) {
                yield return null;
                t += Time.deltaTime;
            }
            _onDeath?.Invoke();
            Respawn();
        }
    }
}
