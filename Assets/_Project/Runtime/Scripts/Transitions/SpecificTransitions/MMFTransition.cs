using System;
using System.Threading;
using Metroma.Transitions;
using MoreMountains.Feedbacks;
using UnityEngine;

namespace Metroma
{
    [RequireComponent(typeof(MMF_Player))]
    public class MMFTransition : ATransition
    {
        [SerializeField, HideInInspector] private MMF_Player _feedbacks;
        private void Reset() {
            _feedbacks = GetComponent<MMF_Player>();
        }
        private void Awake() {
            _feedbacks = GetComponent<MMF_Player>();
        }
        
        protected override async Awaitable TransitionAsync(CancellationToken cancelToken) {
            try {
                await _feedbacks.PlayFeedbacksTask(transform.position);
            }
            catch (OperationCanceledException) {
                
            }
        }
    }
}
