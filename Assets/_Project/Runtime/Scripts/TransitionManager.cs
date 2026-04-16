using System;
using System.Collections;
using NaughtyAttributes;
using UnityEngine;

namespace Metroma
{
    [Serializable]
    public struct MiniGameTransitionData
    {
        public Camera Camera;
        public AdMaterial Material;
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
        public event Action OnTransitionEnded;
        
        public bool TryTransition(MiniGameTransitionData data) {
            if (_isTransitioning) return false;
            CameraHelpers.ApplyConfiguration(data.Camera, data.View.GetConfiguration());
            if (data.IsIn) StartCoroutine(TransiInCoroutine(data));
            else StartCoroutine(TransiOutCoroutine(data));
            return true;
        }
        
        public async Awaitable TryTransitionAsync(MiniGameTransitionData data) {
            if (_isTransitioning) return;
            await CameraHelpers.TransitionToViewAsync(data.Camera, data.View, 0.5f);
            if (data.IsIn) StartCoroutine(TransiInCoroutine(data));
            else StartCoroutine(TransiOutCoroutine(data));
        }
        
        IEnumerator TransiInCoroutine(MiniGameTransitionData data) {
            _isTransitioning = true;
            ActivateZTest(data.Material.Material);
            float t = 0f;
            while (t < data.DurationIn) {
                float p = t / data.DurationIn;
                data.Material.Material.SetFloat(Transition, 1-p);
                t += Time.deltaTime;
                yield return null;
            }
            data.Material.Material.SetFloat(Transition, 0);
            _isTransitioning = false;
            OnTransitionEnded?.Invoke();
        }
        
        IEnumerator TransiOutCoroutine(MiniGameTransitionData data) {
            _isTransitioning = true;
            float t = 0f;
            while (t < data.DurationOut) {
                float p = t / data.DurationOut;
                data.Material.Material.SetFloat(Transition, p);
                t += Time.deltaTime;
                yield return null;
            }
            data.Material.Material.SetFloat(Transition, 1);
            DeactivateZTest(data.Material.Material);
            _isTransitioning = false;
            OnTransitionEnded?.Invoke();
        }
        
        void ActivateZTest(Material mat) {
            mat.SetInt(ZTest, 8);
        }
        
        void DeactivateZTest(Material mat) {
            mat.SetInt(ZTest, 4);
        }
    }
}
