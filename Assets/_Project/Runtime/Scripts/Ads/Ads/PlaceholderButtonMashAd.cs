using System;
using UnityEngine;
using UnityEngine.Events;

namespace Metroma
{
    public class PlaceholderButtonMashAd : AdBase
    {
        private readonly WinCond _winCond = new WinCond();
        [SerializeField] private int _tapNumber;
        private int _amount;
        private bool _won = false;

        [SerializeField] private UnityEvent<int> _buttonPressed;
        [SerializeField] private UnityEvent UnityOnAdEnd;
        public event Action<int> OnButtonPressed;

        public int Amount => _amount;
        public int TapNumber => _tapNumber;
        
        private class WinCond : ICondition<PlaceholderButtonMashAd>
        {
            public bool Evaluate(PlaceholderButtonMashAd context) {
                return context._amount >= context._tapNumber;
            }
        }
        
        public override void StartAd() {
            if (!controllables[0]) {
                Debug.Log("il y a pas de controllable");
                return;
            }
            InputManager.instance.AddControllable(controllables[0]);
            controllables[0].OnActionStart += ButtonPressed;
            controllables[0].IsActive = true;
        }

        private void OnDisable() {
            controllables[0].OnActionStart -= ButtonPressed;
            InputManager.instance.PurgeControllables();
        }

        protected override void OnAdEnd() {
            base.OnAdEnd();
            controllables[0].OnActionStart -= ButtonPressed;
        }
        
        private void ButtonPressed() {
            _amount++;
            _buttonPressed?.Invoke(_amount);
            OnButtonPressed?.Invoke(_amount);
            if (_winCond.Evaluate(this) && !_won) {
                OnAdEnd();
                controllables[0].OnActionStart -= ButtonPressed;
                _won = true;
                UnityOnAdEnd?.Invoke();
            }
        }

        public void End() => OnAdEnd(); //bullshit pour unity event (m'en branle)
    }
}
