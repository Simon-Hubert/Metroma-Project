using System;
using System.Collections.Generic;
using UnityEngine;

namespace Metroma
{
    [RequireComponent(typeof(Collider2D))]
    public class AdCollider : MonoBehaviour
    {
        [SerializeField, HideInInspector] private Collider2D _col;

        private HashSet<Rigidbody2D> _collidingBodies;

        public HashSet<Rigidbody2D> CollidingBodies
        {
            get => _collidingBodies;
        }

        private void Reset() {
            _col = GetComponent<Collider2D>();
        }

        private void OnTriggerEnter2D(Collider2D other) {
            _collidingBodies.Add(other.attachedRigidbody);
        }

        private void OnTriggerExit2D(Collider2D other) {
            _collidingBodies.Remove(other.attachedRigidbody);
        }
    }
}
