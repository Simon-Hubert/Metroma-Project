using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Metroma
{
    public class Credits : MonoBehaviour
    {
        [Header("Return Settings")]
        [SerializeField] private Camera MainMenuCamera;
        [SerializeField] private Camera CreditsCamera;
        [SerializeField] private GameObject CreditsPanel;
        
        
        [Header("Scroll Settings")]
        [SerializeField] private float _speed = 50f;
        [SerializeField, Tooltip("La hauteur Y à laquelle les crédits s'arrêtent et on retourne au menu")]
        private float EndYPosition = 2500f;

        private bool _started;
        private RectTransform _transform;
        private Vector2 _startPos;
        private bool _isInitialized = false;
        
        private void Update()
        {
            if (!_started || _transform == null)
                return;

            _transform.anchoredPosition += Vector2.up * (_speed * Time.unscaledDeltaTime);

            if (_transform.anchoredPosition.y >= EndYPosition)
            {
                StopAndReturn();
                return;
            }

            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                StopAndReturn();
                return;
            }
            if (Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame)
            {
                StopAndReturn();
                return;
            }
        }

        public void Play()
        {
            gameObject.SetActive(true);
            _transform = GetComponent<RectTransform>();
            
            if (!_isInitialized)
            {
                _startPos = _transform.anchoredPosition;
                _isInitialized = true;
            }
            else
            {
                _transform.anchoredPosition = _startPos;
            }
            
            _started = true;
        }

        private void StopAndReturn()
        {
            _started = false;

            if (MainMenuCamera != null)
                MainMenuCamera.gameObject.SetActive(true);

            if (CreditsCamera != null)
                CreditsCamera.gameObject.SetActive(false);

            if (CreditsPanel != null)
                CreditsPanel.SetActive(false);
        }
    }
}
