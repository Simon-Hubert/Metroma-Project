using System;
using UnityEngine;
using UnityEngine.Events;

namespace Metroma
{
    public class WinLoseEvent : MonoBehaviour
    {
        [SerializeReference, ConditionAttribute(ConditionAttribute.ConditionType.Decorator)] private ICondition _winCondition;
        [SerializeReference, ConditionAttribute(ConditionAttribute.ConditionType.Decorator)] private ICondition _loseCondition;

        [SerializeField] private UnityEvent _onWin;
        [SerializeField] private UnityEvent _onLose;
        [SerializeField] private bool _evaluateEveryFrames = true;

        private bool _resolved = false;

        public event Action OnWin;
        public event Action OnLose;

        public bool IsResolved => _resolved;

        public void Evaluate()
        {
            if (_resolved) return;

            if (_winCondition != null && _winCondition.Evaluate())
            {
                Resolve(true);
            }
            else if (_loseCondition != null && _loseCondition.Evaluate())
            {
                Resolve(false);
            }
        }

        private void Resolve(bool win)
        {
            _resolved = true;

            if (win)
            {
                OnWin?.Invoke();
                _onWin?.Invoke();
            }
            else
            {
                OnLose?.Invoke();
                _onLose?.Invoke();
            }
        }

        private void Update()
        {
            if (_evaluateEveryFrames && !_resolved)
            {
                Evaluate();
            }
        }
    }
}
