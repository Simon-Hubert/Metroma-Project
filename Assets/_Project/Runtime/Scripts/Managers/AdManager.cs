using UnityEngine;
using Metroma.Transitions;

namespace Metroma
{
    public class AdManager : MonoBehaviour
    {
        public AdBase CurrentAd { get => _currentAd; }
        
        private AdBase _currentAd;
        
        public void StartAd(AdBase ad, ATransition inATransition, ATransition outATransition)
        {
            _currentAd = ad;
            inATransition.Play();
            
            _currentAd.OnAdEnded += () =>
            {
                outATransition.Play();
            };
        }
    }
}
