using System;
using UnityEngine;

namespace Metroma
{
    [Serializable]
    public class ConditionalEvent : ICondition
    {
        [SerializeField] private ICondition _condition;

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
