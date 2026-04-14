using System;
using UnityEngine;
using UnityEngine.Events;

namespace Metroma
{
    public class AdCollider : MonoBehaviour
    {
        [SerializeField] private UnityEvent _onCollision;
        public event Action<Collider2D> OnCollisionEnter;
        public event Action<Collider2D> OnCollisionExit;
        
        
        private void OnTriggerEnter2D(Collider2D other) {
            OnCollisionEnter?.Invoke(other);
            _onCollision?.Invoke();
        }
        
        private void OnTriggerExit2D(Collider2D other) {
            OnCollisionExit?.Invoke(other);
        }


    }
}
