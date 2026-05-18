using System;
using UnityEngine;

namespace Metroma
{
    [RequireComponent(typeof(MeshRenderer))]
    public class AdMaterial : MonoBehaviour
    {
        private static readonly int BaseMap = Shader.PropertyToID("_BaseMap");
        [SerializeField] private Material mat;
        [SerializeField] private Texture _bindedTexture;
        public Material Material => mat;

        private void Start() {
            mat = Instantiate(mat);
            mat.SetTexture(BaseMap, _bindedTexture);
            GetComponent<MeshRenderer>().material = mat;
        }
    }
}