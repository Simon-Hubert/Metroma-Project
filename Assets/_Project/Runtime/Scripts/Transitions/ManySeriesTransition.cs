using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Metroma.Transitions 
{
    public class ManySeriesTransition : ATransition
    {
        [SerializeField] private List<ATransition> _trans;

        protected override async Awaitable TransitionAsync(CancellationToken cancelToken) {
            try {
                foreach (ATransition t in _trans) {
                    await t.PlayAsync(cancelToken);
                }
            }
            catch (OperationCanceledException) {
                
            }
        }
    }
}
