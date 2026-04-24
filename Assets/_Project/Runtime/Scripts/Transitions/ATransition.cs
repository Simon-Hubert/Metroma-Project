using UnityEngine;
using System;
using System.Threading;
using NaughtyAttributes;

namespace Metroma.Transitions
{
    public abstract class ATransition : MonoBehaviour
    {
        public event Action OnTransitionEnd;
        
        protected abstract Awaitable TransitionAsync();

        public async Awaitable PlayAsync() {
            await TransitionAsync();
            OnTransitionEnd?.Invoke();
        }
    }
}