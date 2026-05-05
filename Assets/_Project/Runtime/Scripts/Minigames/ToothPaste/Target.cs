using System;
using UnityEngine;

namespace Metroma
{
    [ExecuteAlways]
    public class Target : MonoBehaviour
    {
        public bool Validated { get; private set; }
        public Rigidbody2D _targetRB;

        public static event Action OnValidated;

        private void Awake() {
            TargetManager.Handle(this);
        }

        private void OnDestroy() {
            TargetManager.Drop(this);
        }

        public void SetTarget(Rigidbody2D newTarget) {
            _targetRB = newTarget;
        }

        private void OnTriggerEnter2D(Collider2D other) {
            if (other.attachedRigidbody == _targetRB) {
                Validated = true;
                Debug.Log("Validated");
                OnValidated?.Invoke();
            }
        }
    }
}
