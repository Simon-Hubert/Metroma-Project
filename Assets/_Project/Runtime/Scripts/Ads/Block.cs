using System;
using System.Threading;
using Metroma.Transitions;
using UnityEngine;
using Metroma.Utils;

namespace Metroma
{
    public class Block : MonoBehaviour
    {
        [SerializeField] private Sequence _startSequence;
        [SerializeField] private Sequence _endSequence;
        [SerializeField] private ConditionalEvent _endCondition;
        [SerializeField] private Controllable _controllable;
        [SerializeField] private ATransition _transitionIn;
        [SerializeField] private bool _allowInputsBeforeStart;

        private CancellationTokenSource _cancelTokenSrc;

        public event Action OnBlockEnded;

        public void StartBlock() {
            Debug.Log($"{name} started !");
            _endCondition.OnValidated += End;
            _ = StartAsync(AwaitableUtils.ResetToken(ref _cancelTokenSrc));
        }

        public async Awaitable StartAsync(CancellationToken cancelToken) {
            try {
                if (_transitionIn) {
                    await _transitionIn.PlayAsync(cancelToken);
                }

                if (_allowInputsBeforeStart) {
                    _controllable.SubscribeInputs(true);
                    await _startSequence.ExecuteAsync();
                }
                else {
                    await _startSequence.ExecuteAsync();
                    _controllable.SubscribeInputs(true);
                }
            }
            catch (OperationCanceledException) {
                
            }
        }

        public void End() {
            _ = EndAsync();
        }

        public async Awaitable EndAsync() {
            _controllable.IsActive = false;
            _controllable.UnsubscribeInputs();
            await _endSequence.ExecuteAsync();
            OnBlockEnded?.Invoke();
            Debug.Log($"{name} ended !");
        }
    }
}