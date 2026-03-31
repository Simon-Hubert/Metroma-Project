using System;
using UnityEngine;

namespace Metroma
{
    public class RenderTextureManager : MonoBehaviour
    {
        [Serializable]
        private struct RenderTarget
        {
            public RenderTexture texture;
            [HideInInspector] public Camera camera;
        }
        
        [SerializeField] private RenderTarget[] _renderTargets;
        public static RenderTextureManager instance;

        private void Awake() {
            if (instance != null) {
                Debug.LogWarning("Attention il y a deux RenderTextureManager dans la scene");
                Destroy(this);
            }
            else {
                instance = this;
            }
        }

        public bool TryGetTexture(int index, Camera targetCamera, out RenderTexture texture) {
            texture = null;
            if (index >= _renderTargets.Length) return false;
            if (index < 0) return false;
            ref RenderTarget target = ref _renderTargets[index];
            if (!target.texture) return false;
            if (target.camera) {
                Debug.LogWarning("RenderTarget Already in use by : ", target.camera);
                return false;
            }
            texture = target.texture;
            target.camera = targetCamera;
            return true;
        }
    }
}
