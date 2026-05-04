using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Metroma
{
    [RequireComponent(typeof(Collider2D))]
    public class AdCollider : MonoBehaviour
    {
        [SerializeField, HideInInspector] private Collider2D _col;

        [SerializeField] private UnityEvent _onTriggerEnterEvent;
        [SerializeField] private UnityEvent _onTriggerExitEvent;

        private HashSet<Rigidbody2D> _collidingBodies = new HashSet<Rigidbody2D>();

        public HashSet<Rigidbody2D> CollidingBodies
        {
            get => _collidingBodies;
        }

        private void Reset() {
            _col = GetComponent<Collider2D>();
        }

        private void OnTriggerEnter2D(Collider2D other) {
            _collidingBodies.Add(other.attachedRigidbody);
            _onTriggerEnterEvent?.Invoke();
        }

        private void OnTriggerExit2D(Collider2D other) {
            _collidingBodies.Remove(other.attachedRigidbody);
            _onTriggerEnterEvent?.Invoke();
        }
    }
}
