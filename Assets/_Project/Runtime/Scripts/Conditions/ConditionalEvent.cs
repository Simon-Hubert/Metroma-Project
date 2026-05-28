using System;
using UnityEngine;
using UnityEngine.Events;

namespace Metroma
{
    public class ConditionalEvent : MonoBehaviour
    {
        [SerializeReference, ConditionAttribute(ConditionAttribute.ConditionType.Decorator)] private ICondition _condition;
        [SerializeField] private UnityEvent _onValidated;
        [SerializeField] private bool _evaluateEveryFrames = false;
        private bool _wasEvaluated = false;
        
        public event Action OnValidated;
        
        public void Evaluate()
        {
            if (_condition.Evaluate()) {
                _wasEvaluated = true;
                OnValidated?.Invoke();
                _onValidated?.Invoke();
            }
        }

        private void Update() {
            if (_evaluateEveryFrames && !_wasEvaluated) {
                Evaluate();
            }
        }
    }
}
