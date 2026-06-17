using System;
using NaughtyAttributes;
using UnityEngine;

namespace Metroma
{
    public class WaterTrail : MonoBehaviour
    {
        [SerializeField] private Rigidbody2D _player;
        [SerializeField] private float _speedToStop;
        [SerializeField] private float _speedToMax;
        [Space(7)]
        [SerializeField] private TrailRenderer _trail;
        [SerializeField, ReadOnly] private float[] _keysAlphas;
        [Space(7)]
        [SerializeField] private ParticleSystem _waterRing;

        private void Start() {
            _keysAlphas = new float[_trail.colorGradient.alphaKeyCount];
            
            for (int i = 0; i < _trail.colorGradient.alphaKeyCount; i++) {
                _keysAlphas[i] = _trail.colorGradient.alphaKeys[i].alpha;
            }
        }

        private void FixedUpdate() {
            if (!_player) return;
            
            if (_trail) {
                float alpha = Mathf.Lerp(0f, 1f, (_player.linearVelocity.magnitude - _speedToStop) / (_speedToMax - _speedToStop));
                Gradient gradient = _trail.colorGradient;
                
                for (int i = 0; i < _trail.colorGradient.alphaKeyCount; i++) {
                    gradient.alphaKeys[i].alpha = _keysAlphas[i] * alpha;
                }

                _trail.colorGradient = gradient;
            }

            if (_waterRing && _waterRing.gameObject.activeSelf) {
                if (_player.linearVelocity.magnitude >= _speedToStop && !_waterRing.isStopped) {
                    _waterRing.Stop();
                }
                else if (_player.linearVelocity.magnitude < _speedToStop && _waterRing.isStopped) {
                    _waterRing.Play();
                }
            }
        }
    }
}
