using System;
using UnityEngine;

namespace Metroma
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private Camera _cam;
        [SerializeField] private AView _view;

        private void Update() {
            CameraHelpers.ApplyConfiguration(_cam, _view.GetConfiguration());
        }
    }
}
