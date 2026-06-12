using UnityEngine;

namespace Metroma
{
    public class WaitForDuration : ASequencable
    {
        [SerializeField] private float _duration;
        
        public override async Awaitable ExecuteAsync() {
            if (!RequirementsValidated()) return;
            await Awaitable.WaitForSecondsAsync(_duration);
        }
        
        public override bool RequirementsValidated() {
            return _duration > 0;
        }

        private void OnValidate() {
            name = $"Wait for {_duration} sec";
        }
        
#if UNITY_EDITOR
        public override InspectorColor GetInspectorColor() {
            return new InspectorColor
            {
                Background = ColorHelpers.ColorFromOKLCH(BackgroundLightness, CHROMA, 143f),
                Text = ColorHelpers.ColorFromOKLCH(TextLightness, CHROMA, 143f),
            };
        }
#endif
    }
}
