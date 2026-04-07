using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using Metroma.Inputs;

namespace Metroma
{
    public class InputManager : MonoBehaviour
    {
        public static InputManager instance;
        
        [SerializeField] private CaptureInputs _captureInputs;
        [SerializeField, ReadOnly] private List<Controllable> _controllables;

        private void Awake() {
            if (instance != null) {
                Destroy(gameObject);
                return;
            }
            instance = this;
        }

        private void Start() {
            if (_captureInputs == null) {
                if (TryGetComponent<CaptureInputs>(out _captureInputs)) {
                    Debug.LogError("Input Error : CaptureInputs component is missing.");
                    return;
                }
            }
        }
        
        private void Update() {
            SendInputs();
        }

        /// <summary>
        /// Will add the given controllable to the InputManagers's list for it to be used.
        /// The controllable value IsActive will be set at false once added.
        /// </summary>
        /// <param name="controllable">IControllable to add</param>
        /// <param name="key">If null, empty, not set or already existing, will be random.</param>
        /// <returns>Key assigned to controllable. Returns empty string in case of error</returns>
        public bool AddControllable(Controllable controllable, bool activeState = false) {
            if (_controllables == null) {
                _controllables = new List<Controllable>();
            }
            
            // Error proof
            if (controllable == null) {
                Debug.LogError("Cannot add controllable : IControllable is null.");
                return false;
            }

            if (_controllables.Contains(controllable))
            {
                Debug.LogWarning("Cannot add controllable : " + controllable.name + " is already referenced.");
                return true;
            }

            controllable.IsActive = activeState; // false by default
            _controllables.Add(controllable);
            return true;
        }

        public void RemoveControllable(Controllable toRemove) {
            if (_controllables == null) return;
            
            _controllables.Remove(toRemove);
        }
        public void RemoveControllables(Controllable[] toRemove) {
            if (_controllables == null) return;

            foreach (Controllable controllable in toRemove) {
                _controllables.Remove(controllable);
            }
        }
        
        /// <summary>
        /// Will clean the dictionnary of any remaining null IControllable. Call it once in a while.
        /// </summary>
        public void PurgeControllables() {
            for (int i = 0; i < _controllables.Count; i++)
            {
                if (_controllables[i] == null)
                {
                    _controllables.RemoveAt(i);
                }
            }
        }

        private void SendInputs() {
            if (!_captureInputs || 
                _controllables == null || _controllables.Count == 0) return;

            GameplayInputsData inputs = _captureInputs.GetGameplayInputsData();
            
            foreach (Controllable controllable in _controllables)
            {
                if (controllable != null && controllable.IsActive) {
                    controllable.Inputs = inputs;
                }
            }
        }
    }
}
