using System;
using UnityEngine;

namespace Metroma
{
    public class CutToBlackSequence : ASequencable
    {
        [SerializeField] private float _duration;
        
        public override async Awaitable ExecuteAsync() {
            if (!RequirementsValidated()) return;
            BlackScreen.Show();
            await Awaitable.WaitForSecondsAsync(_duration);
            BlackScreen.Hide();
        }

        public override bool RequirementsValidated() {
            return _duration > 0;
        }

        private void OnValidate() {
            name = $"Cut to black for {_duration} sec";
        }
        
#if UNITY_EDITOR
        public override InspectorColor GetInspectorColor() {
            return new InspectorColor
            {
                Background = ColorHelpers.ColorFromOKLCH(BackgroundLightness, CHROMA, 255f),
                Text = ColorHelpers.ColorFromOKLCH(TextLightness, CHROMA, 255f),
            };
        }
#endif
    }
}
