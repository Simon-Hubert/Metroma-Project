using UnityEngine;

namespace Metroma.Transitions 
{
    public class SeriesTransition : ATransition
    {
        [SerializeField] private ATransition _a;
        [SerializeField] private ATransition _b;

        protected override async Awaitable TransitionAsync() {
            await _a.PlayAsync();
            await _b.PlayAsync();
        }
    }
}
