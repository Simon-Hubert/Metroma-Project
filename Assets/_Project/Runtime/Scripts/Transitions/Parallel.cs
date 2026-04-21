using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

namespace Metroma
{
    public class Parallel : ATransition {
        [Header("Transitions to play in parallel")]
        [SerializeField] private List<ATransition> _tList = new List<ATransition>();
        
        public override async Awaitable PlayAsync() {
            if (_tList == null || _tList.Count < 1) {
                isPlaying = false;
                return;
            }
            isPlaying = true;

            var play1 = _t1 != null ? _t1.PlayAsync() : null;
            var play2 = _t2 != null ? _t2.PlayAsync() : null;
            
            while (_t1.GetIsPlaying || _t2.GetIsPlaying) {
                await Awaitable.NextFrameAsync();
            }
            // await play1;
            // await play2;
        }
    }
}