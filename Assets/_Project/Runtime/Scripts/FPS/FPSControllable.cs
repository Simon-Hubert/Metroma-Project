using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Metroma
{
    public class FPSControllable : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform _cameraTransform;

        [Header("Look Settings")]
        [SerializeField] private float _mouseSensitivity = 0.1f;
        [SerializeField] private float _controllerSensitivity = 0.1f;
        [SerializeField] private float _minVerticalAngle = -80f;
        [SerializeField] private float _maxVerticalAngle = 80f;
        [SerializeField] private float _minHorizontalAngle = -80f;
        [SerializeField] private float _maxHorizontalAngle = 80f;
        [SerializeField] private bool _invertY = false;

        [Header("Cursor")]
        [SerializeField] private bool _lockCursorOnStart = true;

        private Vector2 _lookInput;
        private float _pitch;
        private float _yaw;
        private float _roll;
        private float _yawOrigin, _pitchOrigin;

        private PlayerInput _playerInput;

        public void Reset() {
            _yaw = 0;
            _pitch = 0;
        }
        
        private void Awake()
        {
            if (_cameraTransform == null && Camera.main != null)
            {
                _cameraTransform = Camera.main.transform;
            }
            
            if (_cameraTransform != null)
            {
                _yawOrigin = _cameraTransform.eulerAngles.y;
                _roll = _cameraTransform.eulerAngles.z;
                _pitchOrigin = _cameraTransform.localEulerAngles.x;
                
    
                if (_pitch > 180f)
                {
                    _pitch -= 360f;
                }
            }
        }

        private void Start()
        {
            if (_lockCursorOnStart)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        private void OnEnable() {
            Reset();
            _playerInput = GetComponent<PlayerInput>();
        }

        private void Update()
        {
            if (Time.timeScale > 0f)
            {
                RotateCamera();
            }
        }

        public void Look(InputAction.CallbackContext context)
        {
            if(_playerInput.currentControlScheme == "Gamepad") {
                _lookInput = context.ReadValue<Vector2>() * _controllerSensitivity;
            }
            else {
                _lookInput = context.ReadValue<Vector2>() * _mouseSensitivity;
            }
        }

        private void RotateCamera()
        {
            if (_cameraTransform == null)
            {
                return;
            }

            float mouseX = _lookInput.x;
            float mouseY = _lookInput.y;

            _yaw += mouseX;

            if (_invertY)
            {
                _pitch += mouseY;
            }
            else
            {
                _pitch -= mouseY;
            }

            _pitch = Mathf.Clamp(_pitch, _minVerticalAngle, _maxVerticalAngle);
            _yaw = Mathf.Clamp(_yaw, _minHorizontalAngle, _maxHorizontalAngle);
            
            _cameraTransform.localRotation = Quaternion.Euler(_pitchOrigin + _pitch, _yawOrigin + _yaw + 180f, _roll);
        }

    }
}
