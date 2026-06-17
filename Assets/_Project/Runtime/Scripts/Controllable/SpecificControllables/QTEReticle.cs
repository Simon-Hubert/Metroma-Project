using System.Collections;
using NaughtyAttributes;
using UnityEngine;

namespace Metroma
{
    public class QTEReticle : MonoBehaviour
    {
        [Header("Sprites")]
        [SerializeField] private SpriteRenderer _hitCircle;
        [Tooltip("Ring that shrinks from the start diameter to the hit circle border.")]
        [SerializeField] private SpriteRenderer _approachRing;

        [Header("Geometry")]
        [SerializeField, Min(0.01f)] private float _hitDiameter = 1f;
        [SerializeField, Min(0.01f)] private float _approachStartDiameter = 3f;
        [SerializeField] private AnimationCurve _approachCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        [Header("Colors")]
        [SerializeField] private Color _baseColor = Color.white;
        [SerializeField] private Color _approachColor = new Color(1f, 1f, 1f, 0.6f);
        [SerializeField] private Color _successColor = new Color(0.35f, 1f, 0.45f, 1f);
        [SerializeField] private Color _failColor = new Color(1f, 0.35f, 0.35f, 1f);

        [Header("Feedback timings")]
        [SerializeField, Min(0f)] private float _colorLerpDuration = 0.15f;
        [SerializeField, Min(0f)] private float _fadeOutDuration = 0.25f;

        [Header("Bump Anim Params")]
        [Tooltip("Scale multiplier of the bump punch on the hit circle.")]
        [SerializeField, Min(1f)] private float _bumpScale = 1.25f;
        [SerializeField, Min(0.01f)] private float _bumpDuration = 0.2f;
        [SerializeField] private AnimationCurve _bumpCurve =
            new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.4f, 1f), new Keyframe(1f, 0f));

        [Header("Fail feedback")]
        [Tooltip("Scale the hit circle shrinks to when the step fails.")]
        [SerializeField, Min(0f)] private float _failShrinkScale = 0.6f;

        private float _hitSpriteUnit = 1f;
        private float _approachSpriteUnit = 1f;
        private Vector3 _hitBaseScale = Vector3.one;

        private Coroutine _feedbackRoutine;
        private Coroutine _bumpRoutine;

        private void Awake()
        {
            if (_hitCircle != null && _hitCircle.sprite != null)
                _hitSpriteUnit = Mathf.Max(0.0001f, _hitCircle.sprite.bounds.size.x);
            if (_approachRing != null && _approachRing.sprite != null)
                _approachSpriteUnit = Mathf.Max(0.0001f, _approachRing.sprite.bounds.size.x);

            SetActive(false);
        }
        
        public void Show(Vector3 worldPosition)
        {
            StopRoutines();

            transform.position = worldPosition;

            _hitBaseScale = DiameterToScale(_hitDiameter, _hitSpriteUnit);
            ApplyHitCircle(_hitBaseScale, _baseColor);
            SetApproach(0f);

            SetActive(true);
        }
        
        public void SetApproach(float approachPercentage)
        {
            if (_approachRing == null) return;

            float eased = _approachCurve.Evaluate(Mathf.Clamp01(approachPercentage));
            float diameter = Mathf.LerpUnclamped(_approachStartDiameter, _hitDiameter, eased);
            _approachRing.transform.localScale = DiameterToScale(diameter, _approachSpriteUnit);
            _approachRing.color = _approachColor;
        }

        public void Bump()
        {
            if (_hitCircle == null) return;
            if (_bumpRoutine != null) StopCoroutine(_bumpRoutine);
            _bumpRoutine = StartCoroutine(BumpRoutine());
        }

        public void PlaySuccess() => PlayResolve(_successColor, true);

        public void PlayFail() => PlayResolve(_failColor, false);

        public void Hide()
        {
            StopRoutines();
            Destroy(gameObject);
        }
        
        private void PlayResolve(Color target, bool success)
        {
            StopRoutines();
            if (success) Bump();
            _feedbackRoutine = StartCoroutine(ResolveRoutine(target, success));
        }

        private IEnumerator ResolveRoutine(Color target, bool success)
        {
            if (_approachRing != null) _approachRing.enabled = false;

            Color fromColor = _hitCircle != null ? _hitCircle.color : target;
            Vector3 fromScale = _hitCircle != null ? _hitCircle.transform.localScale : _hitBaseScale;
            Vector3 toScale = success ? _hitBaseScale : _hitBaseScale * _failShrinkScale;

            float t = 0f;
            while (t < _colorLerpDuration)
            {
                t += Time.deltaTime;
                float k = _colorLerpDuration > 0f ? t / _colorLerpDuration : 1f;
                if (_hitCircle != null)
                {
                    _hitCircle.color = Color.Lerp(fromColor, target, k);
                    if (!success) _hitCircle.transform.localScale = Vector3.Lerp(fromScale, toScale, k);
                }
                yield return null;
            }

            t = 0f;
            Color startColor = target;
            while (t < _fadeOutDuration)
            {
                t += Time.deltaTime;
                float a = Mathf.Lerp(1f, 0f, _fadeOutDuration > 0f ? t / _fadeOutDuration : 1f);
                SetAlpha(startColor, a);
                yield return null;
            }

            _feedbackRoutine = null;
            Destroy(gameObject);
        }

        private IEnumerator BumpRoutine()
        {
            float t = 0f;
            while (t < _bumpDuration)
            {
                t += Time.deltaTime;
                float punch = _bumpCurve.Evaluate(_bumpDuration > 0f ? t / _bumpDuration : 1f);
                float scale = Mathf.LerpUnclamped(1f, _bumpScale, punch);
                if (_hitCircle != null) _hitCircle.transform.localScale = _hitBaseScale * scale;
                yield return null;
            }
            if (_hitCircle != null) _hitCircle.transform.localScale = _hitBaseScale;
            _bumpRoutine = null;
        }
        
        private void ApplyHitCircle(Vector3 scale, Color color)
        {
            if (_hitCircle == null) return;
            _hitCircle.transform.localScale = scale;
            _hitCircle.color = color;
        }

        private Vector3 DiameterToScale(float diameter, float spriteUnit) =>
            Vector3.one * (diameter / spriteUnit);

        private void SetAlpha(Color reference, float alpha)
        {
            if (_hitCircle != null) _hitCircle.color = new Color(reference.r, reference.g, reference.b, alpha);
            if (_approachRing != null && _approachRing.enabled)
                _approachRing.color = new Color(_approachColor.r, _approachColor.g, _approachColor.b, _approachColor.a * alpha);
        }

        private void SetActive(bool active)
        {
            if (_hitCircle != null) _hitCircle.enabled = active;
            if (_approachRing != null) _approachRing.enabled = active;
        }

        private void StopRoutines()
        {
            if (_feedbackRoutine != null) { StopCoroutine(_feedbackRoutine); _feedbackRoutine = null; }
            if (_bumpRoutine != null) { StopCoroutine(_bumpRoutine); _bumpRoutine = null; }
        }
    }
}
