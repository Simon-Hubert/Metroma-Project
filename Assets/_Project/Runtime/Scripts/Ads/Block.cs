using System;
using System.Threading;
using Metroma.Transitions;
using UnityEngine;
using Metroma.Utils;
using NaughtyAttributes;

namespace Metroma
{
    public class Block : MonoBehaviour
    {
        [Tooltip("If true, the block handles both a win and a lose outcome (Win/Lose Event + Fail Sequence). " +
                 "If false, the block only handles a single win condition (End Condition).")]
        [SerializeField] private bool _handleWinAndLose = false;

        [SerializeField] private Sequence _startSequence;
        [SerializeField] private Sequence _endSequence;
        [Tooltip("Played instead of the end sequence when the block is resolved as a failure. " +
                 "Falls back to the end sequence if left empty.")]
        [SerializeField, ShowIf(nameof(_handleWinAndLose))] private Sequence _failSequence;

        [SerializeField, ShowIf(nameof(_handleWinAndLose))] private WinLoseEvent _winLoseEvent;
        [SerializeField, HideIf(nameof(_handleWinAndLose))] private ConditionalEvent _endCondition;

        [SerializeField] private AControllable _controllable;
        [SerializeField] private ATransition _transitionIn;
        [SerializeField] private bool _allowInputsBeforeStart;

        private CancellationTokenSource _cancelTokenSrc;
        private bool _resolved;

        public event Action OnBlockEnded;

        public void StartBlock() {
            Debug.Log($"{name} started !");
            _resolved = false;

            if (_handleWinAndLose) {
                if (_winLoseEvent != null) {
                    _winLoseEvent.OnWin += Win;
                    _winLoseEvent.OnLose += Lose;
                }
            }
            else if (_endCondition != null) {
                _endCondition.OnValidated += Win;
            }

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

        public void End() => Resolve(true);

        private void Win() => Resolve(true);
        private void Lose() => Resolve(false);

        private void Resolve(bool success) {
            if (_resolved) return;
            _resolved = true;

            if (_handleWinAndLose) {
                if (_winLoseEvent != null) {
                    _winLoseEvent.OnWin -= Win;
                    _winLoseEvent.OnLose -= Lose;
                }
            }
            else if (_endCondition != null) {
                _endCondition.OnValidated -= Win;
            }

            _ = EndAsync(success);
        }

        public async Awaitable EndAsync(bool success) {
            _controllable.IsActive = false;
            _controllable.UnsubscribeInputs();

            Sequence sequence = success ? _endSequence : (_failSequence != null ? _failSequence : _endSequence);
            if (sequence != null) await sequence.ExecuteAsync();

            OnBlockEnded?.Invoke();
            Debug.Log($"{name} ended ! (success: {success})");
        }
    }
}
