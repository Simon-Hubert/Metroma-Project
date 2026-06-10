using UnityEngine;

namespace Metroma
{
    public class CamWiggleSequence : ASequencable
    {
        [SerializeField] private CamWiggleAnchor _anchor;
        [SerializeField] private bool _active;

        public override async Awaitable ExecuteAsync() {
            if (!RequirementsValidated()) return;
            _anchor.Target.enabled = _active;
        }
        
        public override bool RequirementsValidated() {
            return _anchor;
        }

        private void OnValidate() {
            if (!RequirementsValidated()) return;
            name = (_active ? "Activate" : "Deactivate") + $"{_anchor.Target.name}";
        }
        
#if UNITY_EDITOR
        public override InspectorColor GetInspectorColor() {
            return new InspectorColor
            {
                Background = ColorHelpers.ColorFromOKLCH(BackgroundLightness, CHROMA, 126f),
                Text = ColorHelpers.ColorFromOKLCH(TextLightness, CHROMA, 126f),
            };
        }
#endif
    }
}
