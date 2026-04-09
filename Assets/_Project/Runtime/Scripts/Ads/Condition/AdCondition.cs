using System;
using UnityEngine;

namespace Metroma
{
    public abstract class AdCondition : MonoBehaviour
    {
        public abstract bool Evaluate();
    }

    public class And : AdCondition
    {
        private AdCondition _a;
        private AdCondition _b;
        
        private void Start() {
            foreach (Transform child in transform) {
                AdCondition cond = child.GetComponent<AdCondition>();
                if (!cond) continue;
                if (!_a) {
                    _a = cond;
                    continue;
                }
                if (!_b) _b = cond;
                if (_a && _b) break;
            }
            if (!_a || !_b) Debug.Log("Il manque des conditions");
        }

        public override bool Evaluate() {
            return _a.Evaluate() && _b.Evaluate();
        }
    }
    
    public class Or : AdCondition
    {
        private AdCondition _a;
        private AdCondition _b;
        
        private void Start() {
            foreach (Transform child in transform) {
                AdCondition cond = child.GetComponent<AdCondition>();
                if (!cond) continue;
                if (!_a) {
                    _a = cond;
                    continue;
                }
                if (!_b) _b = cond;
                if (_a && _b) break;
            }
            if (!_a || !_b) Debug.Log("Il manque des conditions");
        }

        public override bool Evaluate() {
            return _a.Evaluate() || _b.Evaluate();
        }
    }
    
    public class Not : AdCondition
    {
        private AdCondition _a;
        
        private void Start() {
            foreach (Transform child in transform) {
                AdCondition cond = child.GetComponent<AdCondition>();
                if (cond) {
                    _a = cond;
                    break;
                }
            }
        }

        public override bool Evaluate() {
            return !_a.Evaluate();
        }
    }
}
