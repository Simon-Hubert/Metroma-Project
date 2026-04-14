using System;
using UnityEngine;
using UnityEngine.Events;

namespace Metroma
{
    public class CollisionCondition : AdCondition
    {
        [SerializeField] private Rigidbody2D _body;
        private bool _isColliding;
        [SerializeField] private AdCollider _collider;

        private void OnEnable() {
            _collider.OnCollisionEnter += OnColliderEnter;
            _collider.OnCollisionExit += OnColliderExit;
        }
        
        private void OnDisable() {
            _collider.OnCollisionEnter -= OnColliderEnter;
            _collider.OnCollisionExit -= OnColliderExit;
        }
        
        private void OnColliderExit(Collider2D other) {
            if (other.attachedRigidbody == _body) {
                _isColliding = false;
            }
        }
        
        private void OnColliderEnter(Collider2D other) {
            if (other.attachedRigidbody == _body) {
                _isColliding = true;
            }
        }

        public override bool Evaluate() {
            return _isColliding;
        }
    }
}
