using UnityEngine;

namespace Metroma
{
    public class ScratchCondition : AdCondition
    {
        [SerializeField] private float _target;
        [SerializeField] private ScratchControllable _controllable;

        public override bool Evaluate() {
            return _controllable.TotalDistance >= _target;
        }
    }
}
