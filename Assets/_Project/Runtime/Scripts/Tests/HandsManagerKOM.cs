using System;
using UnityEngine;

namespace Metroma
{
    public class HandsManagerKOM : MonoBehaviour
    {
        [SerializeField] private PlaceholderButtonMashAd _ad;
        [SerializeField] private Transform _handLeft;
        [SerializeField] private Transform _handRight;
        [SerializeField] private Transform _targetLeft;
        [SerializeField] private Transform _targetRight;
        private Vector3 _leftStart;
        private Vector3 _rightStart;

        private void Start() {
            _leftStart = _handLeft.position;
            _rightStart = _handRight.position;
            _ad.OnButtonPressed += OnButtonPressed;
        }

        public void OnButtonPressed(int i) {
            SetPurcentage((float)_ad.Amount/_ad.TapNumber);
        }
        
        public void SetPurcentage(float p) {
            _handLeft.position = Vector3.Lerp(_leftStart, _targetLeft.position, p);
            _handRight.position = Vector3.Lerp(_rightStart, _targetRight.position, p);
        }
    }
}
