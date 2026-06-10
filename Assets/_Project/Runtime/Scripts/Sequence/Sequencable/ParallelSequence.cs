using System.Collections.Generic;
using UnityEngine;

namespace Metroma
{
    public class ParallelSequence : ASequencable
    {
        private readonly List<ASequencable> _sequence = new List<ASequencable>();
        [SerializeField] private float _duration;
        
        private void Awake() {
            foreach (Transform child in transform) {
                if (!child.gameObject.activeSelf) continue;
                ASequencable sequencable = child.GetComponent<ASequencable>();
                if(sequencable) _sequence.Add(sequencable);
            }
        }
        
        public override async Awaitable ExecuteAsync() {
            if (!RequirementsValidated()) return;
            foreach (ASequencable sequencable in _sequence) {
                _ = sequencable.ExecuteAsync();
            }

            await Awaitable.WaitForSecondsAsync(_duration);
        }
        
        
        public override bool RequirementsValidated() {
            return transform.childCount > 0 && _duration > 0;
        }
        
#if UNITY_EDITOR
        public override InspectorColor GetInspectorColor() {
            return new InspectorColor
            {
                Background = ColorHelpers.ColorFromOKLCH(BackgroundLightness, CHROMA, 0f),
                Text = ColorHelpers.ColorFromOKLCH(TextLightness, CHROMA, 0f),
            };
        }
#endif
    }
}
