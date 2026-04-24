using System;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

namespace Metroma.Transitions 
{
    public class SeriesTransition : ATransition {
        [Header("Transitions to play in series")]
        [SerializeField] private List<ATransition> _tList = new List<ATransition>();
        
        public override async Awaitable PlayAsync(CancellationToken cancelToken) {
            if (isPlaying || _tList == null || _tList.Count < 1)
                return;
            
            isPlaying = true;

            try {
                foreach (ATransition t in _tList) {
                    if (t != null) {
                        await t.PlayAsync(cancelToken);
                    }
                }
            }
            catch(OperationCanceledException) {
                Debug.LogWarning($"Transition : Task was cancelled");
            }
            finally {
                isPlaying = false;
            }
        }
    }
}
