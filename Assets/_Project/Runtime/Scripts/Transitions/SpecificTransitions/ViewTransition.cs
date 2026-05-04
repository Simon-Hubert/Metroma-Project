using Metroma.Transitions;
using UnityEngine;

namespace Metroma
{
    public class ViewTransition : ATransition
    {
        [SerializeField] private Camera _cam;
        [SerializeField] private AView _targetView;
        [SerializeField] private float _duration;
        [SerializeField] private AnimationCurve _curve = null;
        
        protected override async Awaitable TransitionAsync() {
            if (_curve.keys.Length > 0) {
                await CameraHelpers.TransitionToViewAsync(_cam, _targetView, _duration, _curve);
            }
            else {
                await CameraHelpers.TransitionToViewAsync(_cam, _targetView, _duration);
            }
        }
    }
}
