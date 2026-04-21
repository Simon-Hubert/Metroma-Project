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
        
        [SerializeField, ReadOnly] private List<AControllable> _controllables;
        [SerializedDictionary] private Dictionary<AControllable, ControllableCallBacks> _controllablesCallBacks = new Dictionary<AControllable, ControllableCallBacks>();

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
        /// Updates inputs in every <see cref="AControllable"/> registered and active.
        /// </summary>
        private GameplayInputsData SendInputs() {
            if (!_captureInputs || 
                _controllables == null || _controllables.Count == 0) return new GameplayInputsData(Vector2.zero, 0, false, 0);

            GameplayInputsData inputs = _captureInputs.GetGameplayInputsData();
            
            foreach (AControllable controllable in _controllables) {
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
            
            foreach (AControllable ctrl in _controllables) {
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
        /// Will add the given <see cref="AControllable"/> to the InputManagers's list for it to be used.
        /// </summary>
        /// <param name="aControllable"><see cref="AControllable"/> to add.</param>
        /// <param name="callbacks"><see cref="ControllableCallBacks"/> of the given <see cref="AControllable"/>.</param>
        /// <param name="activeState">If the <see cref="AControllable"/> should be active when added, false by default.</param>
        /// <returns>true if the action is successful. false if there is an error, or <see cref="AControllable"/> is already set.</returns>
        public bool AddControllable(AControllable aControllable, ControllableCallBacks callbacks, bool activeState = false) {
            if (_controllables == null) {
                _controllables = new List<AControllable>();
            }
            
            // Error proof
            if (aControllable == null) {
                Debug.LogError("Cannot add controllable : IControllable is null.");
                return false;
            }
            if (_controllables.Contains(aControllable)) {
                Debug.LogWarning($"Cannot add controllable : {aControllable.name} is already referenced.");
                return false;
            }

            // CallBacks
            if (_controllablesCallBacks.ContainsKey(aControllable)) {
                Debug.LogWarning($"Callbacks Overwrite : {aControllable.name} already has associated Callbacks, it will be overwrite.");
                _controllablesCallBacks[aControllable] = callbacks;
            }
            else {
                _controllablesCallBacks.Add(aControllable, callbacks);
            }

            aControllable.IsActive = activeState; // false by default
            _controllables.Add(aControllable);
            return true;
        }

        public void RemoveControllable(AControllable toRemove) {
            if (_controllables == null) return;
            
            _controllables.Remove(toRemove);
            _controllablesCallBacks.Remove(toRemove);
        }
        public void RemoveControllables(AControllable[] toRemove) {
            if (_controllables == null) return;

            foreach (AControllable controllable in toRemove) {
                RemoveControllable(controllable);
            }
        }
        
        /// <summary>
        /// Will clean the dictionnary of any remaining null <see cref="AControllable"/>. Call it once in a while.
        /// </summary>
        [Button]
        public void PurgeControllables() {
            for (int i = 0; i < _controllables.Count; i++) {
                if (_controllables[i] == null) _controllables.RemoveAt(i);
            }
            
            foreach (AControllable ctrl in _controllablesCallBacks.Keys) {
                if (ctrl == null || !_controllables.Contains(ctrl)) {
                    if (_controllablesCallBacks.Remove(ctrl)) {
                        Debug.LogError($"Purge Controllables : Cannot remove callbacks for {ctrl?.name}");
                    }
                }
            }
        }
    }
}
