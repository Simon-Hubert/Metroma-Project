using System;
using UnityEngine;

namespace Metroma
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class Tooth : MonoBehaviour, IInitiable
    {
        [SerializeField] private int _amount;
        [SerializeField] private Gradient _colorGradient;
        private bool _validated = false;
        private SpriteRenderer _sr;
        private int _currentAmount = 0;
        private bool _isInit = false;

        public bool Validated => _validated;

        private void Start() {
            _sr = GetComponent<SpriteRenderer>();
        }

        public void Init() {
            Debug.Log($"{name} inited !");
            _sr.color = _colorGradient.Evaluate((float)_currentAmount / _amount);
            _isInit = true;
        }
        
        public void OnHit() {
            if (!_isInit) return;
            if (_currentAmount < _amount) {
                _currentAmount++;
                _sr.color = _colorGradient.Evaluate((float)_currentAmount / _amount);
            }
            else {
                _validated = true;
            }
        }
    }
}
