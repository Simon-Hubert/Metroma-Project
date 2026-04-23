using System;
using UnityEngine;

namespace Metroma
{
    public class ConditionalEvent : MonoBehaviour, ICondition
    {
        [SerializeReference, ConditionAttribute(ConditionAttribute.ConditionType.Decorator)] private ICondition _condition;

        public event Action OnValidated;
        public bool Evaluate()
        {
            if (_condition.Evaluate())
            {
                OnValidated?.Invoke();
                return true;
            }

            return false;
        }
    }
}
