using UnityEngine;

namespace Metroma
{
    public class MashCondition : AdCondition
    {
        [SerializeField] private ButtonMashControllable _controllable;
        [SerializeField] private int _value;
        
        public override bool Evaluate() {
            return _controllable.Number > _value;
        }
    }
}
