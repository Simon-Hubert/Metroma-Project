using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Metroma
{
    public class CameraController : MonoBehaviour
    {
        [field: SerializeField] public Camera Camera { get; set; }
        private CameraConfiguration _configuration;
        public static CameraController instance;

        [SerializeField] private AView _activeView;
        private CameraConfiguration _target;

        private bool _isActive = true;
        public void SetIsActive(bool active) {
            _isActive = active;
        }
        
        private void Awake() {
            if (instance != null) {
                Debug.LogWarning("Y a plusieurs instances de ton singleton (askip)");
                Destroy(this);
                return;
            }
            instance = this;
        }

        private void Update() {
            if(!_isActive) return;
            ApplyConfiguration();
            _target = _activeView.GetConfiguration();
        }
        
        private void ApplyConfiguration() {
            Camera.transform.rotation = _target.GetRotation();
            Camera.transform.position = _target.GetPosition();
            Camera.fieldOfView = _target.Fov;
        }

        public void SetView(AView view) {
            _activeView = view;
        }

        private void OnDrawGizmos() {
            _target.DrawGizmo(Color.magenta);
        }
    }
}

