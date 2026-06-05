using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Metroma
{
    public class VolumeAnchor : MonoBehaviour
    {
        [SerializeField] private PostProcessAnchor _anchor;

        private void Awake() {
            _anchor.Target = GetComponent<Volume>();
        }
    }
}
