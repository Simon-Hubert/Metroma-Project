using UnityEngine;

namespace Metroma
{
    public class CoinScratchManagerKOM : MonoBehaviour
    {
        [SerializeField] private PlaceholderButtonMashAd _ad;
        [SerializeField] private Transform _coin;
        [SerializeField] private Transform _target;
        private Vector3 _coinStart;

        private void Start() {
            _coinStart = _coin.position;
            _ad.OnButtonPressed += OnButtonPressed;
        }

        public void OnButtonPressed(int i) {
            SetPurcentage((float)_ad.Amount/_ad.TapNumber);
        }
        
        public void SetPurcentage(float p) {
            _coin.position = Vector3.Lerp(_coinStart, _target.position, p);
        }
    }
}
