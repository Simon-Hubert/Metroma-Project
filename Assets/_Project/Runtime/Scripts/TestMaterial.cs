using System;
using NaughtyAttributes;
using UnityEngine;

namespace Metroma
{
    
    public class TestMaterial : MonoBehaviour
    {

        [SerializeField] private MeshRenderer _meshRenderer;
        private Material _mat;
        private bool _isA;
        
        private void Start() {
            _mat = _meshRenderer.material;
            _mat = Instantiate(_mat);
            _meshRenderer.material = _mat;
        }

        [Button]
        void ToggleZtest() {
            _isA = !_isA;
            if (!_isA) _mat.SetInt("_ZTest", 4);
            else _mat.SetInt("_ZTest", 8);
        }
    }
}
