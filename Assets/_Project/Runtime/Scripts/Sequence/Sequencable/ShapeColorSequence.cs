using Codice.CM.Client.Differences.Graphic;
using Shapes;
using UnityEngine;
using UnityEngine.Serialization;

namespace Metroma
{
    public class ShapeColorSequence : ASequencable
    {
        [SerializeField] private ShapeRenderer _shape;
        [SerializeField] private float _duration;
        [SerializeField] private Color _StartColor;
        [SerializeField] private Color _EndColor;
        
        public override async Awaitable ExecuteAsync() {
            if (!RequirementsValidated()) return;

            float time = _duration;
            while (time > 0) {
                time -= Time.deltaTime;
                _shape.Color = Color.Lerp(_EndColor, _StartColor, time / _duration);
                await Awaitable.NextFrameAsync();
            }
        }

        public override bool RequirementsValidated() {
            return _duration > 0 && _shape;
        }

        private void OnValidate() {
            name = $"Show {_shape} for {_duration} sec";
        }
        
#if UNITY_EDITOR
        public override InspectorColor GetInspectorColor() {
            return new InspectorColor
            {
                Background = ColorHelpers.ColorFromOKLCH(BackgroundLightness, CHROMA, 320f),
                Text = ColorHelpers.ColorFromOKLCH(TextLightness, CHROMA, 320f),
            };
        }
#endif
    }
}
