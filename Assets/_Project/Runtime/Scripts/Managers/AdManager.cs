using UnityEngine;

namespace Metroma
{
    public class AdManager : MonoBehaviour
    {
        public AdBase CurrentAd { get => _currentAd; }
        
        private AdBase _currentAd;
        
        public async Awaitable StartAd(AdBase ad, Transition inTransition, Transition outTransition)
        {
            _currentAd = ad;
            await inTransition.PlayAsync();
            
            ad.StartAd();
            
            _currentAd.OnAdEnded += () =>
            {
                _ = outTransition.PlayAsync();
            };
        }
    }
}
