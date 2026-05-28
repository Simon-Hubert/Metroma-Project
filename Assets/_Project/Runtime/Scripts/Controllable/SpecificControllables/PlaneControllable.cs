using System;
using UnityEngine;

namespace Metroma
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlaneControllable : AControllable
    {
        [SerializeField] private float _gravity;
        [SerializeField] private float _upForce;
        public float vel { get; private set; }
        
        private Rigidbody2D _rb;

        protected override void Start() {
            base.Start();
            _rb = GetComponent<Rigidbody2D>();
        }

        protected override void FixedUpdate() {
            base.FixedUpdate();
            if (!IsActive) return;
            _rb.linearVelocity += -Vector2.up * (_gravity * Time.fixedDeltaTime);
            vel = _rb.linearVelocity.y;
        }

        /*protected override void InputAction(bool action) {
            base.InputActionStart(action);
            if (!IsActive) return;
            _velocity = Vector2.up * _upForce;
        }*/

        protected override void InputActionPerformed(bool action) {
            base.InputActionPerformed(action);
            if (action) {
                _rb.linearVelocity += (_gravity + _upForce) * Time.deltaTime * Vector2.up;
            }
        }

        private void OnTriggerEnter2D(Collider2D other) {
            if (other.CompareTag("PlaneObstacle")) {
                Debug.Log("Hit");
            }
        }
    }
}
