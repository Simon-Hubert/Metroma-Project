using UnityEngine;

namespace Metroma
{
    public class DistantSequence : ASequencable
    {
        [SerializeField] private Sequence _sequence;
        
        public override async Awaitable ExecuteAsync() {
            if (!RequirementsValidated()) return;
            await _sequence.ExecuteAsync();
        }
        
        public override bool RequirementsValidated() {
            return _sequence;
        }

        public void OnValidate() {
            if (!RequirementsValidated()) return;
            name = $"Play {_sequence.name}";
        }

#if UNITY_EDITOR
        public override InspectorColor GetInspectorColor() {
            return new InspectorColor
            {
                Background = ColorHelpers.ColorFromOKLCH(BackgroundLightness, CHROMA, 135f),
                Text = ColorHelpers.ColorFromOKLCH(TextLightness, CHROMA, 135f),
            };
        }
#endif
    }
}
