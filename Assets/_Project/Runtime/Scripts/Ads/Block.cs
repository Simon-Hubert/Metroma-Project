using System;
using Metroma.Transitions;
using UnityEngine;

namespace Metroma
{
    public class Block : MonoBehaviour
    {
        [SerializeField] private Sequence _startSequence;
        [SerializeField] private Sequence _endSequence;
        [SerializeField] private ConditionalEvent _endCondition;
        [SerializeField] private AControllable _controllable;
        [SerializeField] private ATransition _transitionIn;
        [SerializeField] private bool _allowInputsBeforeStart;

        public event Action OnBlockEnded;

        public void StartBlock() {
            Debug.Log($"{name} started !");
            _endCondition.OnValidated += End;
            _ = StartAsync();
        }

        public async Awaitable StartAsync() {
            if (_transitionIn)
                await _transitionIn.PlayAsync();

            if (_allowInputsBeforeStart) {
                _controllable.SubscribeInputs(true);
                await _startSequence.ExecuteAsync();
            }
            else {
                await _startSequence.ExecuteAsync();
                _controllable.SubscribeInputs(true);
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