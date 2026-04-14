using UnityEngine;

namespace Metroma
{
    public class CoinScratchManagerKOM : MonoBehaviour
    {
        [SerializeField] private PlaceholderButtonMashAd _ad;
        [SerializeField] private Transform _coin;
        [SerializeField] private Transform[] _waypoints;
        [SerializeField] private float _lerpSpeed = 5f;
        
        private Vector3 _coinStart;
        private Vector3 _targetPosition;
        private bool _isMoving = false;

        private void Start() {
            _coinStart = _coin.position;
            _ad.OnButtonPressed += OnButtonPressed;
        }

        public void OnButtonPressed(int i) {
            SetPurcentage((float)_ad.Amount/_ad.TapNumber);
        }
        
        private void Update() {
            if (_isMoving) {
                _coin.position = Vector3.Lerp(_coin.position, _targetPosition, Time.deltaTime * _lerpSpeed);
                
                if (Vector3.Distance(_coin.position, _targetPosition) < 0.01f) {
                    _coin.position = _targetPosition;
                    _isMoving = false;
                }
            }
        }
        
        public void SetPurcentage(float p) {
            if (_waypoints == null || _waypoints.Length == 0) return;
            
            p = Mathf.Clamp01(p);
            
            if (_waypoints.Length == 1) {
                _targetPosition = Vector3.Lerp(_coinStart, _waypoints[0].position, p);
                _isMoving = true;
                return;
            }
            
            float segmentLength = 1f / _waypoints.Length;
            int currentSegment = Mathf.FloorToInt(p / segmentLength);
            float segmentProgress = (p - (currentSegment * segmentLength)) / segmentLength;
            
            if (currentSegment >= _waypoints.Length) {
                _targetPosition = _waypoints[_waypoints.Length - 1].position;
                _isMoving = true;
                return;
            }
            
            Vector3 startPoint = currentSegment == 0 ? _coinStart : _waypoints[currentSegment - 1].position;
            Vector3 endPoint = _waypoints[currentSegment].position;
            
            _targetPosition = Vector3.Lerp(startPoint, endPoint, segmentProgress);
            _isMoving = true;
        }
    }
}
