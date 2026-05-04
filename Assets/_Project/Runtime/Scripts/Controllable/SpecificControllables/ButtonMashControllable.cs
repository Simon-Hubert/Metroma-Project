using UnityEngine;
using UnityEngine.Events;

namespace Metroma
{
    public class ButtonMashControllable : AControllable
    {
        [ConditionParam] private int _value;
        public int Value => _value;

        [SerializeField] private UnityEvent<int> _onValueChanged;
        
        protected override void InputActionStart(bool action) {
            base.InputActionStart(action);
            _value++;
            _onValueChanged?.Invoke(_value);
        }
    }
}
