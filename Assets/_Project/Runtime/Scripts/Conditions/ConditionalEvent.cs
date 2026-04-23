using System;
using UnityEngine;

namespace Metroma
{
    public class ConditionalEvent : MonoBehaviour, ICondition
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
