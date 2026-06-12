using System;
using UnityEngine;

namespace Metroma
{
    public class CameraAnchorSetter : MonoBehaviour
    {
        [SerializeField] private CameraAnchor _anchor;

        private void Awake() {
            _anchor.Target = GetComponent<Camera>();
        }
    }
}
