using System.Collections.Generic;
using UnityEngine;

namespace Metroma
{
    public class AdsManagerKOM : MonoBehaviour
    {
        [SerializeField] private List<AdBase> _ads;
        private int _index = 0;
        
        public AdBase CurrentAd => _ads[_index];
        public AdBase LastAd => _ads[_ads.Count - 1];
        
        public void StartAd()
        {
            _index = 0;
            _ads[0].StartAd();
        }

        public void NextAd()
        {
            _index++;
            _ads[_index].StartAd();
        }
    }
}
