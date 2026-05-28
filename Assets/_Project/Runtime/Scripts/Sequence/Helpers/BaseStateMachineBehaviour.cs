using System;
using UnityEngine;
using UnityEngine.Events;

namespace Metroma
{
    public class BaseStateMachineBehaviour : StateMachineBehaviour
    {
        public event Action OnEnter;
        public event Action OnExit;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            base.OnStateEnter(animator, stateInfo, layerIndex);
            OnEnter?.Invoke();
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            base.OnStateExit(animator, stateInfo, layerIndex);
            OnExit?.Invoke();
        }
    }
}
