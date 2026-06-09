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
            if (RectTrans == null || Mouse.current == null || bIsGlobalParallaxPaused)
                return;

            Vector2 MousePos = Mouse.current.position.ReadValue();

            if (Vector2.SqrMagnitude(MousePos - LastMousePos) < 0.5f)
            {
                Vector2 CurrentTarget = BasePosition + TargetOffset;
                if (Vector2.SqrMagnitude(RectTrans.anchoredPosition - CurrentTarget) < 0.1f)
                {
                    return;
                }
            }

            LastMousePos = MousePos;
            
            float NormalizedX = Mathf.Clamp((MousePos.x / Screen.width) * 2f - 1f, -1f, 1f);
            float NormalizedY = Mathf.Clamp((MousePos.y / Screen.height) * 2f - 1f, -1f, 1f);

            TargetOffset = new Vector2(-NormalizedX * ParallaxStrength, -NormalizedY * ParallaxStrength);
            Vector2 TargetPosition = BasePosition + TargetOffset;

            RectTrans.anchoredPosition = Vector2.Lerp(RectTrans.anchoredPosition, TargetPosition, Time.unscaledDeltaTime * Smoothing);
        }
    }
}
