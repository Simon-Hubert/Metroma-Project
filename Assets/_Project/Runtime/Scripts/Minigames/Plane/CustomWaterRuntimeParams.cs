using System;
using UnityEngine;

namespace Metroma
{
    public class CustomWaterRuntimeParams : MonoBehaviour
    {
        [Header("Reference")]
        [SerializeField] private Material _material;

        [Header("Foam Runtime Params")] 
        [SerializeField] private Color _startFoamColor;
        [SerializeField] private Color _endFoamColor;
        [SerializeField] private float _transitionDuration;


        private Shader _waterShader;
        private float _foamColorTimer;

        private void Start()
        {
            _waterShader = _material.shader;
            _material.SetColor("_FoamColor", _startFoamColor);
            
            _foamColorTimer = 0f;
        }

        private void Update()
        {
            _foamColorTimer += Time.deltaTime;
            
            SetColorWithDuration("_FoamColor", _startFoamColor, _endFoamColor, _transitionDuration, _foamColorTimer);
        }

        private void SetColorWithDuration(string paramName, Color a, Color b, float duration, float timer)
        {
            Color newColor = Color.Lerp(a, b, timer/duration);
            _material.SetColor(paramName, newColor);
        }
    }
}
