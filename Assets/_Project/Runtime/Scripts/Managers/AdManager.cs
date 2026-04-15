using UnityEngine;

namespace Metroma
{
    public class AdManager : MonoBehaviour
    {
        public AdBase CurrentAd { get => _currentAd; }
        
        private AdBase _currentAd;
        
        public void StartAd(AdBase ad, Transition inTransition, Transition outTransition)
        {
            _currentAd = ad;
            _ = inTransition.PlayAsync();
            
            _currentAd.OnAdEnded += () =>
            {
                _ = outTransition.PlayAsync();
            };
        }
    }
}
