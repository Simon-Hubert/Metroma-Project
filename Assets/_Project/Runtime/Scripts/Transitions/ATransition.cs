using UnityEngine;
using System;
using System.Threading;
using NaughtyAttributes;
using UnityEngine.Events;

namespace Metroma.Transitions
{
    public abstract class ATransition : MonoBehaviour
    {
        public event Action OnTransitionEnd;
        [SerializeField] private UnityEvent _onTransitionEnded;
        
        protected abstract Awaitable TransitionAsync();

        public async Awaitable PlayAsync() {
            await TransitionAsync();
            OnTransitionEnd?.Invoke();
            _onTransitionEnded?.Invoke();
        }
    }
}