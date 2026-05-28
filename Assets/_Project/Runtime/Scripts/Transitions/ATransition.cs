using UnityEngine;
using System;
using System.Threading;
using UnityEngine.Events;

namespace Metroma.Transitions
{
    public abstract class ATransition : MonoBehaviour
    {
        [SerializeField] private bool _callEventIfCancellation;
        public event Action OnTransitionStart;
        [SerializeField] private UnityEvent _onTransitionStarted;
        
        public event Action OnTransitionEnd;
        [SerializeField] private UnityEvent _onTransitionEnded;
        
        protected CancellationTokenSource cancelTokenSource;
        
        protected abstract Awaitable TransitionAsync(CancellationToken cancelToken);

        public async Awaitable PlayAsync(CancellationToken cancelToken) {
            try {
                OnTransitionStart?.Invoke();
                _onTransitionStarted?.Invoke();
                
                await TransitionAsync(cancelToken);

                if (!_callEventIfCancellation) {
                    OnTransitionEnd?.Invoke();
                    _onTransitionEnded?.Invoke();
                }
            }
            catch (OperationCanceledException) {
                Debug.Log($"'{name}' : Transition was Cancelled", gameObject);
            }
            finally {
                if (_callEventIfCancellation) {
                    OnTransitionEnd?.Invoke();
                    _onTransitionEnded?.Invoke();
                }
            }
        }
    }
}