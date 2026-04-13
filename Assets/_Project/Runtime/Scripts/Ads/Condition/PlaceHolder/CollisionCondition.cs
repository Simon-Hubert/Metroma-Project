using System;
using UnityEngine;
using UnityEngine.Events;

namespace Metroma
{
    public class CollisionCondition : AdCondition
    {
        [SerializeField] private Rigidbody2D _body;
        private bool _isColliding;
        [SerializeField] private UnityEvent _onCollision;
        
        private void OnTriggerEnter2D(Collider2D other) {
            if (other.attachedRigidbody == _body) {
                _isColliding = true;
                _onCollision?.Invoke();
            }
        }
        
        private void OnTriggerExit2D(Collider2D other) {
            if (other.attachedRigidbody == _body) {
                _isColliding = false;
            }
        }

        public override bool Evaluate() {
            return _isColliding;
        }
    }
}
