using UnityEngine;

namespace Metroma
{
    public class Parallel : ATransition {
        [Header("Transitions to play in parallel")]
        [SerializeField] private ATransition _t1;
        [SerializeField] private ATransition _t2;
        
        public override async Awaitable PlayAsync() {
            if (_t1 == null && _t2 == null)
                return;

            var play1 = _t1 != null ? _t1.PlayAsync() : null;
            var play2 = _t2 != null ? _t2.PlayAsync() : null;

            await play1;
            await play2;
        }
    }
}