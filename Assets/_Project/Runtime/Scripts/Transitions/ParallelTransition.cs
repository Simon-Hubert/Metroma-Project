using System;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;

namespace Metroma.Transitions
{
    public class ParallelTransition : ATransition {
        [SerializeField] private ATransition _a;
        [SerializeField] private ATransition _b;
        
        protected override async Awaitable TransitionAsync() {
            _ = _a.PlayAsync();
            await _b.PlayAsync();
        }
    }
}