using UnityEngine;
using UnityEngine.Events;

namespace Metroma
{
    public class ButtonMashControllable : Controllable
    {
        [ConditionParam] private int _value;
        public int Value => _value;

        [SerializeField] private UnityEvent<int> _onValueChanged;
        
        protected override void InputActionStart() {
            base.InputActionStart();
            _value++;
            _onValueChanged?.Invoke(_value);
            Debug.Log("+");
        }
    }
}
