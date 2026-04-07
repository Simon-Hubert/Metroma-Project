using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using AYellowpaper.SerializedCollections;
using Metroma.Inputs;

namespace Metroma
{
    public class InputManager : MonoBehaviour
    {
        public static InputManager instance;
        
        [SerializeField] private CaptureInputs _captureInputs;
        [SerializedDictionary, ReadOnly] private Dictionary<string, Controllable> _controllables;

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
        public string AddControllable(Controllable controllable, string key = "") {
            if (_controllables == null) {
                _controllables = new Dictionary<string, Controllable>();
            }
            
            // Error proof
            if (controllable == null) {
                Debug.LogError("Cannot add controllable : IControllable is null.");
                return "";
            }

            int secure = 0;
            while (string.IsNullOrEmpty(key) || _controllables.ContainsKey(key) || secure >= 1000) {
                key = Random.Range(0, 1000).ToString();
                secure++;
            }

            controllable.IsActive = false;
            _controllables.Add(key, controllable);
            return key;
        }

        public void RemoveControllable(string key) {
            if (_controllables == null) return;
            
            _controllables.Remove(key);
        }
        public void RemoveControllables(string[] keys) {
            if (_controllables == null) return;
            
            foreach (string key in keys)
            {
                RemoveControllable(key);
            }
        }
        
        /// <summary>
        /// Will clean the dictionnary of any remaining null IControllable. Call it once in a while.
        /// </summary>
        public void PurgeControllables() {
            foreach (string k in _controllables.Keys)
            {
                if (_controllables[k] == null)
                {
                    RemoveControllable(k);
                }
            }
        }

        private void SendInputs() {
            if (!_captureInputs || 
                _controllables == null || _controllables.Count == 0) return;

            GameplayInputsData inputs = _captureInputs.GetGameplayInputsData();
            
            foreach (Controllable controllable in _controllables.Values)
            {
                if (controllable != null && controllable.IsActive) {
                    controllable.Inputs = inputs;
                }
            }
        }
    }
}
