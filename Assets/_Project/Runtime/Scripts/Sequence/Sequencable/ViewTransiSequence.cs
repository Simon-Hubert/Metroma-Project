using System;
using UnityEngine;

namespace Metroma
{
    public class ViewTransiSequence : ASequencable
    {
        [SerializeField] private Camera _cam;
        [SerializeField] private AView _target;
        [SerializeField] private float _duration;
        [SerializeField] private AnimationCurve _curve;
        
        public override async Awaitable ExecuteAsync() {
            if (_curve.keys.Length > 0) {
                await CameraHelpers.TransitionToViewAsync(_cam, _target, _duration, _curve);
            }
            else {
                await CameraHelpers.TransitionToViewAsync(_cam, _target, _duration);
            }
        }

        public override bool RequirementsValidated() {
            return _cam && _target && _duration > 0;
        }

        private void OnValidate() {
            if (!RequirementsValidated()) return;
            name = $"Transi {_cam.name} to {_target.name}";
        }
        
#if UNITY_EDITOR
        public override InspectorColor GetInspectorColor() {
            return new InspectorColor
            {
                Background = ColorHelpers.ColorFromOKLCH(BackgroundLightness, CHROMA, 270f),
                Text = ColorHelpers.ColorFromOKLCH(TextLightness, CHROMA, 270f),
            };
        }
#endif
    }
}
