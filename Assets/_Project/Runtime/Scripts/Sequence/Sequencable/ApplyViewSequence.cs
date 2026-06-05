using System;
using NaughtyAttributes;
using UnityEngine;

namespace Metroma
{
    public class ApplyViewSequence : ASequencable
    {
        [SerializeField] private Camera _cam;
        [SerializeField] private AView _view;
        [SerializeField] private bool _forDuration;
        [SerializeField, ShowIf("_forDuration")] private float _duration;
        
        public override async Awaitable ExecuteAsync() {
            if (!RequirementsValidated()) return;
            CameraHelpers.ApplyConfiguration(_cam, _view.GetConfiguration());
            if (_forDuration) {
                await Awaitable.WaitForSecondsAsync(_duration);
            }
        }

        public override bool RequirementsValidated() {
            return _view && _cam;
        }

        private void OnValidate() {
            if (!RequirementsValidated()) return;
            name = $"Apply {_view.name} to {_cam.name}";
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
