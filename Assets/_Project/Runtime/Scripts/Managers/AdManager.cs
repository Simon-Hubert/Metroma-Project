using UnityEngine;

namespace Metroma
{
    public class AdManager : MonoBehaviour
    {
        public AdBase CurrentAd { get => _currentAd; }
        
        private AdBase _currentAd;
        
        public void StartAd(AdBase ad, ATransition inATransition, ATransition outATransition)
        {
            _currentAd = ad;
            _ = inATransition.PlayAsync();
            
            _currentAd.OnAdEnded += () =>
            {
                _ = outATransition.PlayAsync();
            };
        }
    }
}
