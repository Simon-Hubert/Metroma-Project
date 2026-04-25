using System;
using UnityEngine;
using UnityEngine.Events;

namespace Metroma
{
    public class ConditionalEvent : MonoBehaviour
    {
        [SerializeReference, ConditionAttribute(ConditionAttribute.ConditionType.Decorator)] private ICondition _condition;
        [SerializeField] private UnityEvent _onValidated;
        
        public event Action OnValidated;
        
        public void Evaluate()
        {
            if (_condition.Evaluate())
            {
                OnValidated?.Invoke();
                _onValidated?.Invoke();
            }
        }
    }
}
