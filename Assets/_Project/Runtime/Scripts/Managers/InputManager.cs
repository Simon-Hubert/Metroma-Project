using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using Metroma.Inputs;
using AYellowpaper.SerializedCollections;

namespace Metroma
{
    public delegate void InputV2CallBack(Vector2 _vector2);
    public delegate void InputBCallBack(bool _bool);

    public struct ControllableCallBacks {
        public InputV2CallBack moveStartCallBack { get; }
        public InputV2CallBack moveEndCallBack { get; }
        public InputBCallBack actionStartCallBack { get; }
        public InputBCallBack actionEndCallBack { get; }

        public ControllableCallBacks(InputV2CallBack moveStart, InputV2CallBack moveEnd, InputBCallBack actionStart, InputBCallBack actionEnd)
        {
            moveStartCallBack = moveStart;
            moveEndCallBack = moveEnd;
            actionStartCallBack = actionStart;
            actionEndCallBack = actionEnd;
        } 
    }

    public enum InputsCallBackType {
        NONE = 0,
        MOVE_START = 1,
        MOVE_END = 2,
        ACTION_START = 3,
        ACTION_END = 4
    }
    
    public class InputManager : MonoBehaviour
    {
        public static InputManager instance;
        
        [SerializeField] private CaptureInputs _captureInputs;
        
        [SerializeField, ReadOnly] private List<Controllable> _controllables;
        [SerializedDictionary] private Dictionary<Controllable, ControllableCallBacks> _controllablesCallBacks = new Dictionary<Controllable, ControllableCallBacks>();

        private void Awake() {
            if (instance != null) {
                Destroy(gameObject);
                return;
            }
            
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start() {
            if (_captureInputs == null) {
                if (TryGetComponent<CaptureInputs>(out _captureInputs)) {
                    Debug.LogError("Input Error : CaptureInputs component is missing.");
                    return;
                }
            }

            _captureInputs.OnMoveStart += MoveStartCall;
            _captureInputs.OnMoveEnd += MoveEndCall;
            _captureInputs.OnActionStart += ActionStartCall;
            _captureInputs.OnActionEnd += ActionEndCall;
        }

        private void OnDisable() {
            _captureInputs.OnMoveStart -= MoveStartCall;
            _captureInputs.OnMoveEnd -= MoveEndCall;
            _captureInputs.OnActionStart -= ActionStartCall;
            _captureInputs.OnActionEnd -= ActionEndCall;
        }

        private void Update() {
            
            SendInputs();
        }
        
        /// <summary>
        /// Updates inputs in every <see cref="Controllable"/> registered and active.
        /// </summary>
        private GameplayInputsData SendInputs() {
            if (!_captureInputs || 
                _controllables == null || _controllables.Count == 0) return new GameplayInputsData(Vector2.zero, 0, false, 0);

            GameplayInputsData inputs = _captureInputs.GetGameplayInputsData();
            
            foreach (Controllable controllable in _controllables) {
                if (controllable != null && controllable.IsActive) {
                    controllable.Inputs = inputs;
                }
            }

            return inputs;
        }

        #region OnInputsCall
        private void MoveStartCall() {
            SendCallBack(InputsCallBackType.MOVE_START);
        }
        private void MoveEndCall() {
            SendCallBack(InputsCallBackType.MOVE_END);
        }
        private void ActionStartCall() {
            SendCallBack(InputsCallBackType.ACTION_START);
        }
        private void ActionEndCall() {
            SendCallBack(InputsCallBackType.ACTION_END);
        }

        /// <summary>
        /// Check and call every CallBack function registered.
        /// </summary>
        /// <param name="type">NONE by default</param>
        private void SendCallBack(InputsCallBackType type = InputsCallBackType.NONE) {
            GameplayInputsData inputs = SendInputs();
            
            foreach (Controllable ctrl in _controllables) {
                if (ctrl != null && ctrl.IsActive && _controllablesCallBacks.TryGetValue(ctrl, out ControllableCallBacks callBacks)) {
                    switch (type) {
                        case InputsCallBackType.MOVE_START :
                            callBacks.moveStartCallBack(inputs.move);
                            break;
                        case InputsCallBackType.MOVE_END :
                            callBacks.moveEndCallBack(inputs.move);
                            break;
                        case InputsCallBackType.ACTION_START :
                            callBacks.actionStartCallBack(inputs.action);
                            break;
                        case InputsCallBackType.ACTION_END :
                            callBacks.actionEndCallBack(inputs.action);
                            break;
                        
                        default:
                            break;
                    }
                }
            }
        }
        #endregion
        
        /// <summary>
        /// Will add the given <see cref="Controllable"/> to the InputManagers's list for it to be used.
        /// </summary>
        /// <param name="controllable"><see cref="Controllable"/> to add.</param>
        /// <param name="callbacks"><see cref="ControllableCallBacks"/> of the given <see cref="Controllable"/>.</param>
        /// <param name="activeState">If the <see cref="Controllable"/> should be active when added, false by default.</param>
        /// <returns>true if the action is successful. false if there is an error, or <see cref="Controllable"/> is already set.</returns>
        public bool AddControllable(Controllable controllable, ControllableCallBacks callbacks, bool activeState = false) {
            if (_controllables == null) {
                _controllables = new List<Controllable>();
            }
            
            // Error proof
            if (controllable == null) {
                Debug.LogError("Cannot add controllable : IControllable is null.");
                return false;
            }
            if (_controllables.Contains(controllable)) {
                Debug.LogWarning($"Cannot add controllable : {controllable.name} is already referenced.");
                return false;
            }

            // CallBacks
            if (_controllablesCallBacks.ContainsKey(controllable)) {
                Debug.LogWarning($"Callbacks Overwrite : {controllable.name} already has associated Callbacks, it will be overwrite.");
                _controllablesCallBacks[controllable] = callbacks;
            }
            else {
                _controllablesCallBacks.Add(controllable, callbacks);
            }

            controllable.IsActive = activeState; // false by default
            _controllables.Add(controllable);
            return true;
        }

        public void RemoveControllable(Controllable toRemove) {
            if (_controllables == null) return;
            
            _controllables.Remove(toRemove);
            _controllablesCallBacks.Remove(toRemove);
        }
        public void RemoveControllables(Controllable[] toRemove) {
            if (_controllables == null) return;

            foreach (Controllable controllable in toRemove) {
                RemoveControllable(controllable);
            }
        }
        
        /// <summary>
        /// Will clean the dictionnary of any remaining null <see cref="Controllable"/>. Call it once in a while.
        /// </summary>
        [Button]
        public void PurgeControllables() {
            for (int i = 0; i < _controllables.Count; i++) {
                if (_controllables[i] == null) _controllables.RemoveAt(i);
            }
            
            foreach (Controllable ctrl in _controllablesCallBacks.Keys) {
                if (ctrl == null || !_controllables.Contains(ctrl)) {
                    if (_controllablesCallBacks.Remove(ctrl)) {
                        Debug.LogError($"Purge Controllables : Cannot remove callbacks for {ctrl?.name}");
                    }
                }
            }
        }
    }
}
