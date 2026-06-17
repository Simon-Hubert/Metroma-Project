using UnityEngine;

namespace Metroma
{
    public class SpriteOpacitySequence : ASequencable
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private float _duration;
        [SerializeField] private AnimationCurve _curve;
        
        public override async Awaitable ExecuteAsync() {
            if (!RequirementsValidated()) return;
            float t = 0;
            Color col = _spriteRenderer.color;
            col.a = _curve.Evaluate(0);
            _spriteRenderer.color = col;
            while (t < _duration) {
                await Awaitable.EndOfFrameAsync();
                t += Time.deltaTime;
                float p = t / _duration;
                p = _curve.Evaluate(p);
                Color color = _spriteRenderer.color;
                color.a = p;
                _spriteRenderer.color = color;
            }
            col.a = _curve.Evaluate(1);
            _spriteRenderer.color = col;
        }

        public override bool RequirementsValidated() {
            return _spriteRenderer && _duration > 0;
        }

        private void OnValidate() {
            if (!RequirementsValidated()) return;
            name = $"Animate Opacity of {_spriteRenderer.name}";
        }
        
#if UNITY_EDITOR
        public override InspectorColor GetInspectorColor() {
            return new InspectorColor
            {
                Background = ColorHelpers.ColorFromOKLCH(BackgroundLightness, CHROMA, 270f),
                Text = ColorHelpers.ColorFromOKLCH(TextLightness, CHROMA, 270f),
            };
        }
#endif
    }

}
