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

        public ControllableCallBacks GetCallbacks {
            get {
                InputCallBack moveStart = InputMoveStart;
                InputCallBack moveEnd = InputMoveEnd;
                InputCallBack actionStart = InputActionStart;
                InputCallBack actionEnd = InputActionEnd;
                
                return new ControllableCallBacks(moveStart, moveEnd, actionStart, actionEnd);
            }
        }
        
        protected virtual void InputMoveStart() {
            OnMoveStartUnity.Invoke();
            if (showDebugLog) Debug.Log($"Controllable {name} : Move Start");
        }
        protected virtual void InputMoveEnd() {
            OnMoveEndUnity.Invoke();
            if (showDebugLog) Debug.Log($"Controllable {name} : Move End");
        }

        protected virtual void InputActionStart() {
            OnActionStartUnity.Invoke();
            if (showDebugLog) Debug.Log($"Controllable {name} : Action Start");
        }
        protected virtual void InputActionEnd() {
            OnActionEndUnity.Invoke();
            if (showDebugLog) Debug.Log($"Controllable {name} : Action End");
        }

        #endregion

        #region Sub/Unsub
        /// <summary>
        /// Subscribes this <see cref="Controllable"/> to receive Inputs.
        /// </summary>
        /// <returns>true if <see cref="Controllable"/> is successfully subscribed. false if it fails to subscribe, or is already subscribed.</returns>
        public bool SubscribeInputs() {
            return SubscribeInputs(false);
        }
        /// <summary>
        /// Subscribes this <see cref="Controllable"/> to receive Inputs.
        /// <param name="activeAtStart">Whether this <see cref="Controllable"/> should be active when Subscribed.</param>
        /// </summary>
        /// <returns>true if <see cref="Controllable"/> is successfully subscribed. false if it fails to subscribe, or is already subscribed.</returns>
        public bool SubscribeInputs(bool activeAtStart) {
            return InputManager.instance.AddControllable(this, GetCallbacks, activeAtStart);
        }

        /// <summary>
        /// Unsubscribe this <see cref="Controllable"/> to stop receiving Inputs.
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
