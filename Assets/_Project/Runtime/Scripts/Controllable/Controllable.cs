using System;
using UnityEngine;
using Metroma.Inputs;
using UnityEngine.Events;
using NaughtyAttributes;

namespace Metroma
{
    /// <summary>
    /// Parent class of every Controllable.
    /// You'll have to Inherit this class in order to modify its behavior.
    /// </summary>
    public class Controllable : MonoBehaviour
    {
        [SerializeField, ReadOnly] private bool _isActive;
        /// <summary>
        /// Whether the Controllable captures inputs or not
        /// </summary>
        public bool IsActive
        {
            get => _isActive;
            set {
                _isActive = value;
                
                if (!_isActive) {
                    inputs = new GameplayInputsData(Vector2.zero, 0.0f, false, 0.0f);
                }
                else {
                    // Check for AFK
                }
            }
        }
        
        protected GameplayInputsData lastInputs;
        protected GameplayInputsData inputs;

        public GameplayInputsData Inputs {
            get => inputs;
            set
            {
                lastInputs = inputs;
                inputs = value;
                
                if (lastInputs.move == Vector2.zero && Inputs.move != Vector2.zero) {
                    OnMoveStartUnity?.Invoke();
                    OnMoveStart?.Invoke();
                } // Move Start
                else if (lastInputs.move != Vector2.zero && Inputs.move == Vector2.zero) {
                    OnMoveEndUnity?.Invoke();
                    OnMoveEnd?.Invoke();
                } // Move End
            
                if (lastInputs.action == false && Inputs.action == true) {
                    OnActionStartUnity?.Invoke();
                    OnActionStart?.Invoke();
                } // Action Start
                else if (lastInputs.action == true && Inputs.action == false) {
                    OnActionEndUnity?.Invoke();
                    OnActionEnd?.Invoke();
                }  // Action End
            }
        }
        
        public Transform GetTransform { get => transform; }
        
        /// <summary>
        /// Do not register both event on the same callback, this may cause a double call.
        /// </summary>
        #region Events
        [Foldout("Events")] public UnityEvent OnMoveStartUnity;
        public event Action OnMoveStart;
        [Foldout("Events")] public UnityEvent OnMoveEndUnity;
        public event Action OnMoveEnd;

        [Foldout("Events")] public UnityEvent OnActionStartUnity;
        public event Action OnActionStart;
        [Foldout("Events")] public UnityEvent OnActionEndUnity;
        public event Action OnActionEnd;
        #endregion

        protected virtual void Update() {

        }
        protected virtual void FixedUpdate() {

        }
        protected virtual void LateUpdate() {

        }
        
        #if UNITY_EDITOR
        [Button]
        protected void Editor_SwitchActiveState()
        {
            IsActive = !IsActive;
        }
        [Button]
        protected void Editor_AddControllable()
        {
            InputManager.instance.AddControllable(this, true);
        }
        [Button]
        protected void Editor_RemoveControllable()
        {
            InputManager.instance.RemoveControllable(this);
        }
        #endif
    }
}
