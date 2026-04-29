using UnityEngine;
using System;
using System.Threading;

namespace Metroma.Transitions 
{
    public class SeriesTransition : ATransition
    {
        [SerializeField] private ATransition _a;
        [SerializeField] private ATransition _b;

        protected override async Awaitable TransitionAsync(CancellationToken cancelToken) {
            try {
                await _a.PlayAsync(cancelToken);
                await _b.PlayAsync(cancelToken);
            }
            catch (OperationCanceledException) {
                
            }
        }
    }
}
