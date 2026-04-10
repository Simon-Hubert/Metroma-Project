using System;
using NaughtyAttributes;
using UnityEngine;

namespace Metroma
{
    public class ButtonMashControllable : Controllable
    {
        [field: SerializeField, ReadOnly] public int Number { get; private set; }
        [SerializeField] private InputManager _inputManager;
        
        private void OnEnable() {
            OnActionStart += ButtonPressed;
            _inputManager.AddControllable(this);
        }
        
        private void OnDisable() {
            OnActionStart -= ButtonPressed;
        }
        
        private void ButtonPressed() {
            Number++;
        }

    }
}
