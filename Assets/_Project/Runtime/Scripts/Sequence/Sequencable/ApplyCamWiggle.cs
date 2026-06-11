using UnityEngine;

namespace Metroma
{
    public class ApplyCamWiggle : ASequencable
    {
        [SerializeField] private CamWiggleAnchor _anchor;
        [SerializeField] private Vector3 _wiggleAmplitude;
        [SerializeField] private bool _activate;

        public override async Awaitable ExecuteAsync() {
            if (!RequirementsValidated()) return;
            _anchor.Target.enabled = _activate;
            _anchor.Target.PositionWiggleProperties.AmplitudeMax = _wiggleAmplitude;
        }
        
        public override bool RequirementsValidated() {
            return _anchor;
        }

        private void OnValidate() {
            if (!RequirementsValidated()) return;
            name = $"Apply new wiggle to {_anchor.name}";
        }
        
#if UNITY_EDITOR
        public override InspectorColor GetInspectorColor() {
            return new InspectorColor
            {
                Background = ColorHelpers.ColorFromOKLCH(BackgroundLightness, CHROMA, 140f),
                Text = ColorHelpers.ColorFromOKLCH(TextLightness, CHROMA, 140f),
            };
        }
#endif
    }
}
