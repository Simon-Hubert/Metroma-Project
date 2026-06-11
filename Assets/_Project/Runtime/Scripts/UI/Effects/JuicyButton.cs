using UnityEngine;
using UnityEngine.EventSystems;


namespace Metroma.UI.Effects
{
    public class JuicyButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, ISelectHandler, IDeselectHandler, ISubmitHandler
    {
        [Header("Juicy Scales")]
        [SerializeField] private float HoverScale = 1.05f;
        [SerializeField] private float PressScale = 0.9f;

        [Header("Timing")]
        [SerializeField] private float ClickDelay = 0.3f;

        [Header("Spring Physics")]
        [SerializeField] private float SpringForce = 250f;
        [SerializeField] private float Damping = 15f;

        private Vector3 BaseScale;
        private float TargetScaleMultiplier = 1f;
        private float CurrentScaleMultiplier = 1f;
        private float Velocity = 0f;

        private UnityEngine.UI.Button _button;
        private UnityEngine.UI.Button.ButtonClickedEvent _originalOnClick;
        private bool _isClicking = false;

        private void Awake()
        {
            BaseScale = transform.localScale;
            _button = GetComponent<UnityEngine.UI.Button>();
        }

        private void Start()
        {
            if (_button != null && ClickDelay > 0f)
            {
                StartCoroutine(HijackClickRoutine());
            }
        }

        private System.Collections.IEnumerator HijackClickRoutine()
        {
            yield return new WaitForEndOfFrame();

            _originalOnClick = _button.onClick;
            _button.onClick = new UnityEngine.UI.Button.ButtonClickedEvent();
            _button.onClick.AddListener(OnInterceptedClick);
        }

        private async void OnInterceptedClick()
        {
            if (_isClicking)
                return;

            _isClicking = true;

            await PlayClickEffectAsync();

            _originalOnClick?.Invoke();
            
            _isClicking = false;
        }

        private void OnEnable()
        {
            CurrentScaleMultiplier = 1f;
            
            if (UnityEngine.EventSystems.EventSystem.current != null && UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject == gameObject)
            {
                TargetScaleMultiplier = HoverScale;
            }
            else
            {
                TargetScaleMultiplier = 1f;
            }
        }


        private void Update()
        {
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


        // --- Gamepad / Keyboard Navigation ---

        public void OnSelect(BaseEventData eventData)
        {
            TargetScaleMultiplier = HoverScale;
        }

        public void OnDeselect(BaseEventData eventData)
        {
            TargetScaleMultiplier = 1f;
        }

        public void OnSubmit(BaseEventData eventData)
        {
            CurrentScaleMultiplier = PressScale;
            TargetScaleMultiplier = HoverScale;
        }


        // --- Utils for Code ---

        public async Awaitable PlayClickEffectAsync()
        {
            CurrentScaleMultiplier = PressScale;
            TargetScaleMultiplier = HoverScale;
            
            if (ClickDelay > 0f)
            {
                float elapsed = 0f;
                while (elapsed < ClickDelay)
                {
                    elapsed += Time.unscaledDeltaTime;
                    await Awaitable.NextFrameAsync();
                }
            }
        }
    }
}
