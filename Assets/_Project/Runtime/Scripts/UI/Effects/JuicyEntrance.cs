using UnityEngine;


namespace Metroma.UI.Effects
{
    public class JuicyEntrance : MonoBehaviour
    {
        public enum EntranceType 
        { 
            ScaleUp, 
            SlideFromBottom,
            SlideFromRight
        }


        [Header("Animation Settings")]
        [SerializeField, Tooltip("Type d'animation d'apparition")]
        private EntranceType _entranceType = EntranceType.SlideFromBottom;
        
        [SerializeField, Tooltip("Distance de départ pour les animations de type Slide (en pixels)")]
        private float _slideOffset = 1000f;

        [Header("Spring Physics")]
        [SerializeField] private float SpringForce = 250f;
        [SerializeField] private float Damping = 15f;

        private Vector3 _baseScale;
        private Vector2 _basePosition;
        private RectTransform _rectTransform;

        private float _currentValue = 0f;
        private float _velocity = 0f;


        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            if (_rectTransform != null)
            {
                _baseScale = _rectTransform.localScale;
                _basePosition = _rectTransform.anchoredPosition;
            }
            else
            {
                _baseScale = transform.localScale;
            }

            // Sécurité : si l'objet a été sauvegardé avec un scale de 0 dans l'éditeur, on force à 1
            if (_baseScale == Vector3.zero)
            {
                _baseScale = Vector3.one;
            }
        }


        private void OnEnable()
        {
            Play();
        }

        public void Play()
        {
            _currentValue = 0f;
            _velocity = 0f;

            if (_entranceType == EntranceType.ScaleUp)
            {
                if (_rectTransform != null) _rectTransform.localScale = Vector3.zero;
                else transform.localScale = Vector3.zero;
            }
            else if (_entranceType == EntranceType.SlideFromBottom && _rectTransform != null)
            {
                _rectTransform.anchoredPosition = _basePosition - new Vector2(0, _slideOffset);
            }
            else if (_entranceType == EntranceType.SlideFromRight && _rectTransform != null)
            {
                _rectTransform.anchoredPosition = _basePosition + new Vector2(_slideOffset, 0);
            }
        }


        private void Update()
        {
            float targetValue = 1f;
            float force = (targetValue - _currentValue) * SpringForce;
            
            _velocity += force * Time.unscaledDeltaTime;
            _velocity *= Mathf.Clamp01(1f - Damping * Time.unscaledDeltaTime);
            _currentValue += _velocity * Time.unscaledDeltaTime;

            if (_entranceType == EntranceType.ScaleUp)
            {
                if (_rectTransform != null)
                {
                    _rectTransform.localScale = _baseScale * _currentValue;
                }
                else
                {
                    transform.localScale = _baseScale * _currentValue;
                }
                
            }
            else if (_entranceType == EntranceType.SlideFromBottom && _rectTransform != null)
            {
                _rectTransform.anchoredPosition = Vector2.LerpUnclamped(_basePosition - new Vector2(0, _slideOffset), _basePosition, _currentValue);
            }
            else if (_entranceType == EntranceType.SlideFromRight && _rectTransform != null)
            {
                _rectTransform.anchoredPosition = Vector2.LerpUnclamped(_basePosition + new Vector2(_slideOffset, 0), _basePosition, _currentValue);
            }
        }
    }
}
