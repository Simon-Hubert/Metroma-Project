using System;
using UnityEngine;
using UnityEngine.Events;

namespace Metroma
{
    public class CutToBlackSequence : ASequencable
    {
        [SerializeField] private float _duration;

        [SerializeField] private UnityEvent OnStart;
        [SerializeField] private UnityEvent OnEnd;
        
        public override async Awaitable ExecuteAsync() {
            if (!RequirementsValidated()) return;
            
            OnStart?.Invoke();
            BlackScreen.Show();
            await Awaitable.WaitForSecondsAsync(_duration);
            OnEnd?.Invoke();
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
