using System;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

namespace Metroma.Transitions
{
    public class ManyParallelTransition : ATransition {
        [SerializeField] private List<ATransition> _trans;
        
        protected override async Awaitable TransitionAsync(CancellationToken cancelToken) {
            try {
                List<Awaitable> tasks = new List<Awaitable>();
                
                foreach (ATransition t in _trans) {
                    if (t != null) tasks.Add(t.PlayAsync(cancelToken));
                }
                
                int remaining = tasks.Count;
                foreach (var task in tasks) {
                    _ = AwaitTask(task);
                }
                
                async Awaitable AwaitTask(Awaitable t) {
                    try {
                        await t;
                    }
                    finally {
                        remaining--;
                    }
                }
                
                while (remaining > 0) {
                    await Awaitable.NextFrameAsync(cancelToken);
                }
            }
            catch (OperationCanceledException) {
                
            }
        }
    }
}
