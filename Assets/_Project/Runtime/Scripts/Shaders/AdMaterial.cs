using System;
using UnityEngine;

namespace Metroma
{
    [RequireComponent(typeof(MeshRenderer))]
    public class AdMaterial : MonoBehaviour
    {
        [SerializeField] private Material mat;
        public Material Material => mat;

        private void Start() {
            mat = Instantiate(mat);
            GetComponent<MeshRenderer>().material = mat;
        }
    }
}
