using UnityEngine;
using UnityEngine.EventSystems;

namespace Metroma.UI.Effects
{
    public class JuicyButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [Header("Juicy Scales")]
        [SerializeField] private float HoverScale = 1.05f;
        [SerializeField] private float PressScale = 0.9f;

        [Header("Spring Physics")]
        [SerializeField] private float SpringForce = 250f;
        [SerializeField] private float Damping = 15f;

        private Vector3 BaseScale;
        private float TargetScaleMultiplier = 1f;
        private float CurrentScaleMultiplier = 1f;
        private float Velocity = 0f;

        private void Start()
        {
            BaseScale = transform.localScale;
            CurrentScaleMultiplier = 1f;
            TargetScaleMultiplier = 1f;
        }


        private void Update()
        {
            // Physique de ressort (Spring) pour l'élasticité
            float Force = (TargetScaleMultiplier - CurrentScaleMultiplier) * SpringForce;
            Velocity += Force * Time.unscaledDeltaTime;
            Velocity *= Mathf.Clamp01(1f - Damping * Time.unscaledDeltaTime);
            CurrentScaleMultiplier += Velocity * Time.unscaledDeltaTime;

            transform.localScale = BaseScale * CurrentScaleMultiplier;
        }


        public void OnPointerEnter(PointerEventData eventData)
        {
            TargetScaleMultiplier = HoverScale;
        }


        public void OnPointerExit(PointerEventData eventData)
        {
            TargetScaleMultiplier = 1f;
        }


        public void OnPointerDown(PointerEventData eventData)
        {
            TargetScaleMultiplier = PressScale;
        }


        public void OnPointerUp(PointerEventData eventData)
        {
            // S'il est relâché tout en étant survolé, on retourne au HoverScale, sinon au scale de base
            if (eventData.pointerCurrentRaycast.gameObject == gameObject || 
                (eventData.pointerCurrentRaycast.gameObject != null && eventData.pointerCurrentRaycast.gameObject.transform.IsChildOf(transform)))
            {
                TargetScaleMultiplier = HoverScale;
            }
            else
            {
                TargetScaleMultiplier = 1f;
            }
        }
    }
}
