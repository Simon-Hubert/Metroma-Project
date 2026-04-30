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
                if (_a != null) await _a.PlayAsync(cancelToken);
                if (_b != null) await _b.PlayAsync(cancelToken);
            }
            catch (OperationCanceledException) {
                
            }
        }
    }
}
