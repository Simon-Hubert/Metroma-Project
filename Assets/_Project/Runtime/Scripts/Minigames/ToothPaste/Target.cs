using System;
using UnityEngine;
using UnityEngine.Events;

namespace Metroma
{
    [ExecuteAlways]
    public class Target : MonoBehaviour
    {
        public bool Validated { get; private set; }
        private Tooth _tooth;
        public Rigidbody2D _targetRB;

        public static event Action OnValidated;

        private void Awake() {
            TargetManager.Handle(this);
            _tooth = GetComponent<Tooth>();
        }

        private void OnDestroy() {
            TargetManager.Drop(this);
        }

        public void SetTarget(Rigidbody2D newTarget) {
            _targetRB = newTarget;
        }

        private void OnTriggerEnter2D(Collider2D other) {
            if (other.attachedRigidbody == _targetRB) {
                Debug.Log("Hit");
                _tooth.OnHit();
                if (_tooth.Validated) {
                    Validated = true;
                    OnValidated?.Invoke();
                }
            }
        }
    }
}
