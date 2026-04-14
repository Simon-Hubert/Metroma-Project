using UnityEngine;

namespace Metroma
{
    public class ScratchCondition : AdCondition
    {
        [SerializeField] private float _target;
        public float Target => _target;
        [SerializeField] private ScratchControllable _controllable;

        public override bool Evaluate() {
            return _controllable.TotalDistance >= _target;
        }
    }
}
