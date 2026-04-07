using System;
using UnityEngine;
using Metroma.Inputs;
using UnityEngine.Events;

namespace Metroma
{
    /// <summary>
    /// Parent class of every Controllable.
    /// You'll have to Inherit this class in order to modify its behavior.
    /// </summary>
    public class Controllable : MonoBehaviour
    {
        private bool _isActive;

        /// <summary>
        /// Whether the Controllable captures inputs or not
        /// </summary>
        public bool IsActive
        {
            get => _isActive;
            set {
                _isActive = value;
                
                if (!_isActive) {
                    _inputs = new GameplayInputsData();
                }
                else {
                    // Check for AFK
                }
            }
        }
        
        protected GameplayInputsData _lastInputs;
        protected GameplayInputsData _inputs;

        public GameplayInputsData Inputs {
            get => _inputs;
            set
            {
                _lastInputs = _inputs;
                _inputs = value;
                
                if (_lastInputs.move == Vector2.zero && Inputs.move != Vector2.zero) OnMoveStart?.Invoke();
                else if (_lastInputs.move != Vector2.zero && Inputs.move == Vector2.zero) OnMoveEnd?.Invoke();
            
                if (_lastInputs.action == false && Inputs.action == true) OnActionStart?.Invoke();
                else if (_lastInputs.action == true && Inputs.action == false) OnActionEnd?.Invoke();
            }
        }
        
        public Transform GetTransform { get => transform; }
        
        public UnityEvent OnMoveStart;
        public UnityEvent OnMoveEnd;

        public UnityEvent OnActionStart;
        public UnityEvent OnActionEnd;
    }
}
