using System;
using UnityEngine;

namespace Metroma
{
    public class ConditionalEvent : MonoBehaviour
    {
        private AdCondition _condition;
        public event Action OnValidated;
        
        private void Start() {
            foreach (Transform child in transform) {
                AdCondition cond = child.GetComponent<AdCondition>();
                if (cond) {
                    _condition = cond;
                    break;
                }
            }
        }

        public void Evaluate() {
            if (_condition.Evaluate()) {
                OnValidated?.Invoke();
                Debug.Log($"{name} validated !");
            }
        }
    }
    
}
