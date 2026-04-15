using System;
using UnityEngine;

namespace Metroma
{
    public class AdBase
    {
        [SerializeField] private ConditionalEvent _winCond;

        public event Action OnAdStarted;
        public event Action OnAdEnded;
        
        public virtual void Start()
        {
            OnAdStarted?.Invoke();
            _winCond.OnValidated += End;
        }
        
        protected virtual void End()
        {
            OnAdEnded?.Invoke();
        }
    }
}
