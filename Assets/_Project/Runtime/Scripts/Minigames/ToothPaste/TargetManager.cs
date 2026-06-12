using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Metroma
{
    public class TargetManager : AEvaluatable
    {
        [SerializeField] private Rigidbody2D _targetRB;
        private static readonly List<Target> _targets = new List<Target>();

        [SerializeField] private UnityEvent _onValidatedAny;

        public static void Handle(Target instance) {
            _targets.Add(instance);
        }
        
        public static void Drop(Target instance) {
            _targets.Remove(instance);
        }

        private void Start() {
            foreach (Target target in _targets) {
                target.SetTarget(_targetRB);
            }

            Target.OnValidated += _onValidatedAny.Invoke;
        }

        public override bool Evaluate() {
            return _targets.All(target => target.Validated);
        }

        private void OnDrawGizmos() {
            foreach (Target target in _targets) {
                Gizmos.DrawLine(target.transform.position, transform.position);
            }
        }
    }
}
