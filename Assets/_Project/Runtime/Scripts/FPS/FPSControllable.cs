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

        private void Awake()
        {
            if (_cameraTransform == null && Camera.main != null)
            {
                _cameraTransform = Camera.main.transform;
            }

            _yaw = transform.eulerAngles.y;

            if (_cameraTransform != null)
            {
                _pitch = _cameraTransform.localEulerAngles.x;

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

        private void Update()
        {
            RotateCamera();
        }

        public void Look(InputAction.CallbackContext context)
        {
            Debug.Log(context.ReadValue<Vector2>());
            _lookInput = context.ReadValue<Vector2>();
        }

        private void RotateCamera()
        {
            if (_cameraTransform == null)
            {
                return;
            }

            float mouseX = _lookInput.x * _mouseSensitivity;
            float mouseY = _lookInput.y * _mouseSensitivity;

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
            
            _cameraTransform.localRotation = Quaternion.Euler(_pitch, _yaw, 0f);
        }

    }
}
