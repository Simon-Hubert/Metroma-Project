using System;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;

namespace Metroma.Transitions
{
    public class ParallelTransition : ATransition {
        [Header("Transitions to play in parallel")]
        [SerializeField] private List<ATransition> _tList = new List<ATransition>();
        
        public override async Awaitable PlayAsync(CancellationToken cancelToken) {
            if (isPlaying || _tList == null || _tList.Count < 1)
                return;

            isPlaying = true;

            try {
                List<Awaitable> tasks = new List<Awaitable>();
                
                foreach (ATransition t in _tList) {
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
            catch(OperationCanceledException) {
                Debug.LogWarning($"Transition : Task was cancelled");
            }
            finally {
                isPlaying = false;
            }
        }

        // private bool IsAnyPlaying() {
        //     foreach (ATransition t in _tList) {
        //         if (t != null && t.GetIsPlaying) return true;
        //     }
        //     return false;
        // }
    }
}