using MoreMountains.Feedbacks;
using UnityEngine;

namespace Metroma
{
    public class CamWiggleStrengthSequence : ASequencable
    {
        [SerializeField] private CamWiggleAnchor _anchor;
        [SerializeField] private AnimationCurve _curve;
        [SerializeField] private float _duration;
        
        public override async Awaitable ExecuteAsync() {
            float t = 0;
            Vector3 originalAmplitude = _anchor.Target.PositionWiggleProperties.AmplitudeMax;
            while (t < _duration) {
                await Awaitable.EndOfFrameAsync();
                t += Time.deltaTime;
                float p = _curve.Evaluate(t / _duration);
                _anchor.Target.PositionWiggleProperties.AmplitudeMax = originalAmplitude * p;
            }
            _anchor.Target.PositionWiggleProperties.AmplitudeMax = originalAmplitude * _curve.Evaluate(1);
        }
        
        public override bool RequirementsValidated() {
            return _anchor;
        }

        private void OnValidate() {
            if (!RequirementsValidated()) return;
            name = $"Animate Wiggle on {_anchor.name}";
        }
        
#if UNITY_EDITOR
        public override InspectorColor GetInspectorColor() {
            return new InspectorColor
            {
                Background = ColorHelpers.ColorFromOKLCH(BackgroundLightness, CHROMA, 110f),
                Text = ColorHelpers.ColorFromOKLCH(TextLightness, CHROMA, 110f),
            };
        }
#endif
    }
}
