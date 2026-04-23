using System;
using UnityEngine;

namespace Metroma
{
    [Serializable]
    public class CollisionCondition : ICondition
    {
        [SerializeField] private AdCollider _collider;
        [SerializeField] private Rigidbody2D _rb;
        
        public bool Evaluate() {
            return _collider.CollidingBodies.Contains(_rb);
        }
    }
}
