using System;
using UnityEngine;
using System.Threading;

namespace Metroma.Transitions
{
    public class ParallelTransition : ATransition {
        [SerializeField] private ATransition _a;
        [SerializeField] private ATransition _b;
        
        protected override async Awaitable TransitionAsync(CancellationToken cancelToken) {
            try {
                _ = _a.PlayAsync(cancelToken);
                await _b.PlayAsync(cancelToken);
            }
            catch (OperationCanceledException) {
                
            }
        }
    }
}