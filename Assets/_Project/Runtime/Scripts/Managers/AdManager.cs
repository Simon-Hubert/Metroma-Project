using UnityEngine;
using Metroma.Transitions;

namespace Metroma
{
    public static class AdManager
    {
        public static async Awaitable StartAd(AdBase ad, ATransition inATransition, ATransition outATransition)
        {
            await inATransition.PlayAsync();
            
            ad.StartAd();
            
            ad.OnAdEnded += () =>
            {
                outATransition.PlayAsync();
            };
        }
    }
}
