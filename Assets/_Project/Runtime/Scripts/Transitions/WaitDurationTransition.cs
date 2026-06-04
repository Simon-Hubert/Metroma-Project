using System.Threading;
using System;
using Metroma.Transitions;
using UnityEngine;

namespace Metroma
{
    public class WaitDurationTransition : ATransition
    {
        [SerializeField] private float _duration;
        
        protected override async Awaitable TransitionAsync(CancellationToken cancelToken) {
            try {
                await Awaitable.WaitForSecondsAsync(_duration, cancelToken);
            }
            catch (OperationCanceledException) {

            }
        }
    }
}