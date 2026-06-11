using UnityEngine;
using UnityEngine.InputSystem;


namespace Metroma.UI.Effects
{
    public class UIParallax : MonoBehaviour
    {
        [Header("Parallax Settings")]
        [SerializeField, Tooltip("La force du déplacement (en pixels).")]
        private float ParallaxStrength = 15f;
        
        [SerializeField, Tooltip("La vitesse à laquelle l'élément suit la souris.")]
        private float Smoothing = 5f;


        private RectTransform RectTrans;
        private Vector2 BasePosition;


        public static bool bIsGlobalParallaxPaused = false;

        private Vector2 TargetOffset;
        private Vector2 LastMousePos;

        private bool _isUsingMouse = true;
        private GameObject _lastSelectedObj;


        private void Start()
        {
            RectTrans = GetComponent<RectTransform>();
            if (RectTrans != null)
            {
                BasePosition = RectTrans.anchoredPosition;
            }
        }

        private void Update()
        {
            if (RectTrans == null || bIsGlobalParallaxPaused)
                return;

            Vector2 InputPos = new Vector2(Screen.width / 2f, Screen.height / 2f);
            
            if (Mouse.current != null)
            {
                Vector2 currentMousePos = Mouse.current.position.ReadValue();
                if ((currentMousePos - LastMousePos).sqrMagnitude > 2f)
                {
                    _isUsingMouse = true;
                }

                LastMousePos = currentMousePos;
            }

            GameObject currentSelected = UnityEngine.EventSystems.EventSystem.current != null ? UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject : null;
            if (currentSelected != null && currentSelected != _lastSelectedObj)
            {
                _isUsingMouse = false;
                _lastSelectedObj = currentSelected;
            }

            if (_isUsingMouse && Mouse.current != null)
            {
                InputPos = LastMousePos;
            }
            else if (currentSelected != null)
            {
                RectTransform selectedRect = currentSelected.GetComponent<RectTransform>();
                if (selectedRect != null)
                {
                    Canvas rootCanvas = currentSelected.GetComponentInParent<Canvas>();
                    Camera eventCam = (rootCanvas != null && rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay) 
                        ? (rootCanvas.worldCamera != null ? rootCanvas.worldCamera : Camera.main) 
                        : null;
                    
                    if (eventCam != null)
                    {
                        InputPos = RectTransformUtility.WorldToScreenPoint(eventCam, selectedRect.position);
                    }
                    else
                    {
                        InputPos = selectedRect.position;
                    }
                }
            }
            else if (Mouse.current != null)
            {
                InputPos = LastMousePos;
            }

            float NormalizedX = Mathf.Clamp((InputPos.x / Screen.width) * 2f - 1f, -1f, 1f);
            float NormalizedY = Mathf.Clamp((InputPos.y / Screen.height) * 2f - 1f, -1f, 1f);

            TargetOffset = new Vector2(-NormalizedX * ParallaxStrength, -NormalizedY * ParallaxStrength);
            Vector2 TargetPosition = BasePosition + TargetOffset;

            RectTrans.anchoredPosition = Vector2.Lerp(RectTrans.anchoredPosition, TargetPosition, Time.unscaledDeltaTime * Smoothing);
        }
    }
}
