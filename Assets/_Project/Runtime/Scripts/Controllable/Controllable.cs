using System;
using UnityEngine;
using Metroma.Inputs;
using UnityEngine.Events;

namespace Metroma
{
    public class Controllable : MonoBehaviour
    {
        private bool _isActive;
        public bool IsActive { get => _isActive; set => _isActive = value; }

        protected GameplayInputsData _lastInputs;
        public GameplayInputsData Inputs { get; set; }
        
        public Transform GetTransform { get => transform; }
        
        public UnityEvent OnMoveStart;
        public UnityEvent OnMoveEnd;

        public UnityEvent OnActionStart;
        public UnityEvent OnActionEnd;

        protected virtual void Update() {
            if (_lastInputs.move == Vector2.zero && Inputs.move != Vector2.zero) OnMoveStart?.Invoke();
            else if (_lastInputs.move != Vector2.zero && Inputs.move == Vector2.zero) OnMoveEnd?.Invoke();
            
            if (_lastInputs.action == false && Inputs.action == true) OnActionStart?.Invoke();
            else if (_lastInputs.action == true && Inputs.action == false) OnActionEnd?.Invoke();

            _lastInputs = Inputs;
        }
    }
}
