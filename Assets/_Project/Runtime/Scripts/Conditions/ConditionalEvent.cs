using System;
using UnityEngine;

namespace Metroma
{
    public class ConditionalEvent : MonoBehaviour
    {
        [SerializeReference, ConditionAttribute(ConditionAttribute.ConditionType.Decorator)] private ICondition _condition;

        public event Action OnValidated;
        
        public void Evaluate()
        {
            if (_condition.Evaluate())
            {
                OnValidated?.Invoke();
            }
        }
    }
}
