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
            CameraHelpers.ApplyConfiguration(_cam, _view.GetConfiguration());
            if (_forDuration) {
                await Awaitable.WaitForSecondsAsync(_duration);
            }
        }

        private void OnValidate() {
            if (!_view || !_cam) return;
            name = $"Apply {_view.name} to {_cam.name}";
        }
    }
}
