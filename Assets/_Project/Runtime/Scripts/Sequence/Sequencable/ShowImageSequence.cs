using UnityEngine;

namespace Metroma
{
    public class ShowImageSequence : ASequencable
    {
        [SerializeField] private Sprite _sprite;
        [SerializeField] private float _duration;
        
        public override async Awaitable ExecuteAsync() {
            if (!RequirementsValidated()) return;
            FullScreenImage.Show(_sprite);
            await Awaitable.WaitForSecondsAsync(_duration);
            FullScreenImage.Hide();
        }

        public override bool RequirementsValidated() {
            return _duration > 0 && _sprite;
        }

        private void OnValidate() {
            name = $"Show {_sprite} for {_duration} sec";
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
