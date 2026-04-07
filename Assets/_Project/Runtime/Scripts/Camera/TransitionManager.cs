using System;
using System.Collections;
using NaughtyAttributes;
using UnityEngine;

namespace Metroma
{
    struct MiniGameTransitionData
    {
        public Camera Camera;
        public Material Material;
        public AView View;
        public float DurationIn;
        public float DurationOut;
        public bool IsIn;
    }
    
    public class TransitionManager : MonoBehaviour
    {
        private static readonly int ZTest = Shader.PropertyToID("_ZTest");
        private static readonly int Transition = Shader.PropertyToID("_Transition");
        private bool _isTransitioning;
        
        bool TryTransition(MiniGameTransitionData data) {
            if (_isTransitioning) return false;
            CameraHelpers.ApplyConfiguration(data.Camera, data.View.GetConfiguration());
            if (data.IsIn) StartCoroutine(TransiInCoroutine(data));
            else StartCoroutine(TransiOutCoroutine(data));
            return true;
        }
        
        IEnumerator TransiInCoroutine(MiniGameTransitionData data) {
            _isTransitioning = true;
            ActivateZTest(data.Material);
            float t = 0f;
            while (t < data.DurationIn) {
                float p = t / data.DurationIn;
                data.Material.SetFloat(Transition, 1-p);
                t += Time.deltaTime;
                yield return null;
            }
            data.Material.SetFloat(Transition, 0);
            _isTransitioning = false;
        }
        
        IEnumerator TransiOutCoroutine(MiniGameTransitionData data) {
            _isTransitioning = true;
            float t = 0f;
            while (t < data.DurationOut) {
                float p = t / data.DurationOut;
                data.Material.SetFloat(Transition, p);
                t += Time.deltaTime;
                yield return null;
            }
            data.Material.SetFloat(Transition, 1);
            DeactivateZTest(data.Material);
            _isTransitioning = false;
        }
        
        
        
        void ActivateZTest(Material mat) {
            mat.SetInt(ZTest, 4);
        }
        
        void DeactivateZTest(Material mat) {
            mat.SetInt(ZTest, 8);
        }
    }
}
