using System;
using System.Collections;
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
    public abstract class AControllable : MonoBehaviour
    {
#if UNITY_EDITOR
        [SerializeField] private bool activeAtStart = false;
#endif
        [SerializeField] protected bool showDebugLog = false;
        
        [Space(7)]
        [SerializeField, ReadOnly] private bool _isActive;
        /// <summary>
        /// Whether the Controllable captures inputs or not
        /// </summary>
        public virtual bool IsActive
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
                
                if (Inputs.move != Vector2.zero) InputMovePerformed(Inputs.move);
                if (Inputs.action) InputActionPerformed(Inputs.action);
            }
        }
        
        
        /// <summary>
        /// Do not register both event on the same callback, this may cause a double call.
        /// TODO : Rework this ascpect to not implement public events in this class on refacto. Callback functions could be a solution
        /// </summary>
        #region Events
        [Obsolete("May not be used in the futur"), Foldout("Events")] public UnityEvent OnMoveStartUnity;
        [Obsolete] public event Action OnMoveStart;
        [Obsolete("May not be used in the futur"), Foldout("Events")] public UnityEvent OnMoveEndUnity;
        [Obsolete]public event Action OnMoveEnd;

        [Obsolete("May not be used in the futur"), Foldout("Events")] public UnityEvent OnActionStartUnity;
        [Obsolete]public event Action OnActionStart;
        [Obsolete("May not be used in the futur"), Foldout("Events")] public UnityEvent OnActionEndUnity;
        [Obsolete]public event Action OnActionEnd;
        #endregion

        protected virtual void OnEnable() {
            IsActive = false;
        }

        protected virtual void OnDisable() {
            IsActive = false;
        }

        protected virtual void Start() {
#if UNITY_EDITOR
            if (activeAtStart) Editor_AddControllable();
#endif
        }

        protected virtual void Update() {

        }
        protected virtual void FixedUpdate() {

        }
        protected virtual void LateUpdate() {

        }

        #region Inputs Callback

        protected ControllableCallBacks GetCallbacks {
            get {
                InputV2CallBack moveStart = InputMoveStart;
                InputV2CallBack moveEnd = InputMoveEnd;
                InputBCallBack actionStart = InputActionStart;
                InputBCallBack actionEnd = InputActionEnd;
                
                return new ControllableCallBacks(moveStart, moveEnd, actionStart, actionEnd);
            }
        }
        
        protected virtual void InputMoveStart(Vector2 move) {
            OnMoveStartUnity.Invoke();
            if (showDebugLog) Debug.Log($"Controllable {name} : Move Start");
        }
        protected virtual void InputMovePerformed(Vector2 move) {
            if (showDebugLog) Debug.Log($"Controllable {name} : Move Performed");
        }
        protected virtual void InputMoveEnd(Vector2 move) {
            OnMoveEndUnity.Invoke();
            if (showDebugLog) Debug.Log($"Controllable {name} : Move End");
        }

        protected virtual void InputActionStart(bool action) {
            OnActionStartUnity.Invoke();
            if (showDebugLog) Debug.Log($"Controllable {name} : Action Start");
        }
        protected virtual void InputActionPerformed(bool action) {
            if (showDebugLog) Debug.Log($"Controllable {name} : Move Performed");
        }
        protected virtual void InputActionEnd(bool action) {
            OnActionEndUnity.Invoke();
            if (showDebugLog) Debug.Log($"Controllable {name} : Action End");
        }

        #endregion

        #region Sub/Unsub
        /// <summary>
        /// Subscribes this <see cref="AControllable"/> to receive Inputs.
        /// </summary>
        /// <returns>true if <see cref="AControllable"/> is successfully subscribed. false if it fails to subscribe, or is already subscribed.</returns>
        public bool SubscribeInputs() {
            return SubscribeInputs(false);
        }
        /// <summary>
        /// Subscribes this <see cref="AControllable"/> to receive Inputs.
        /// <param name="activeAtStart">Whether this <see cref="AControllable"/> should be active when Subscribed.</param>
        /// </summary>
        /// <returns>true if <see cref="AControllable"/> is successfully subscribed. false if it fails to subscribe, or is already subscribed.</returns>
        public bool SubscribeInputs(bool activeAtStart) {
            return InputManager.instance.AddControllable(this, GetCallbacks, activeAtStart);
        }

        /// <summary>
        /// Unsubscribe this <see cref="AControllable"/> to stop receiving Inputs.
        /// </summary>
        public void UnsubscribeInputs() {
            InputManager.instance.RemoveControllable(this);
        }
        #endregion
        
        #if UNITY_EDITOR
        [Button]
        protected void Editor_SwitchActiveState() {
            IsActive = !IsActive;
        }
        [Button]
        protected void Editor_AddControllable() {
            SubscribeInputs(true);
        }
        [Button]
        protected void Editor_RemoveControllable() {
            UnsubscribeInputs();
        }
        #endif
    }
}
