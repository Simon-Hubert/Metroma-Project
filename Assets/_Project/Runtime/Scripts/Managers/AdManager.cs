using UnityEngine;
using Metroma.Transitions;
using Metroma.Utils;
using System.Threading;

namespace Metroma
{
    public static class AdManager
    {
        private static CancellationTokenSource _cancelTokenSrc;
        
        public static async Awaitable StartAd(AdBase ad, ATransition inATransition, ATransition outATransition) {
            await inATransition.PlayAsync(AwaitableUtils.ResetToken(ref _cancelTokenSrc));
            
            ad.StartAd();
            
            ad.OnAdEnded += () => {
                outATransition.PlayAsync(AwaitableUtils.ResetToken(ref _cancelTokenSrc));
            };
        }
    }
}
