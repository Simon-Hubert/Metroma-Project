using System;
using System.Collections;
using NaughtyAttributes;
using UnityEngine;

namespace Metroma
{
    
    public class TestMaterial : MonoBehaviour
    {

        [SerializeField] private MeshRenderer _meshRenderer;
        [SerializeField] private float _transitionDuration = 2.5f;
        private Material _mat;
        private bool _isA;
        
        private void Start() {  
            _mat = _meshRenderer.material;
            _mat = Instantiate(_mat);
            _meshRenderer.material = _mat;
            _mat.SetFloat("_Transition", 1);
        }

        void ToggleZtest() {
            _isA = !_isA;
            if (!_isA) _mat.SetInt("_ZTest", 4);
            else _mat.SetInt("_ZTest", 8);
        }

        [Button]
        void TransiIn() {
            StartCoroutine(TransiInCoroutine());
        }

        [Button]
        void TransiOut() {
            StartCoroutine(TransiOutCoroutine());
        }

        IEnumerator TransiInCoroutine() {
            ToggleZtest();
            float t = 0f;
            while (t < _transitionDuration) {
                float p = t / _transitionDuration;
                _mat.SetFloat("_Transition", 1-p);
                t += Time.deltaTime;
                yield return null;
            }
            _mat.SetFloat("_Transition", 0);
        }
        
        IEnumerator TransiOutCoroutine() {
            float t = 0f;
            while (t < _transitionDuration) {
                float p = t / _transitionDuration;
                _mat.SetFloat("_Transition", p);
                t += Time.deltaTime;
                yield return null;
            }
            _mat.SetFloat("_Transition", 1);
            ToggleZtest();
        }
    }
}
