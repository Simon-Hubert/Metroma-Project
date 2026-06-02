using UnityEngine;

namespace Metroma
{
    public class ActivateObject : ASequencable
    {
        [SerializeField] private GameObject _object;
        [SerializeField] private bool _active;
        
        public override async Awaitable ExecuteAsync() {
            if (!RequirementsValidated()) return;
            _object.SetActive(_active);
        }

        public override bool RequirementsValidated() {
            return _object;
        }
        
#if UNITY_EDITOR
        public override InspectorColor GetInspectorColor() {
            return new InspectorColor
            {
                Background = ColorHelpers.ColorFromOKLCH(BackgroundLightness, CHROMA, 47f),
                Text = ColorHelpers.ColorFromOKLCH(TextLightness, CHROMA, 47f),
            };
        }
#endif
    }
}
