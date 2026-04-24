using Metroma.Transitions;
using UnityEngine;

namespace Metroma
{
    public class ViewTransition : ATransition
    {
        [SerializeField] private Camera _cam;
        [SerializeField] private AView _targetView;
        [SerializeField] private float _duration;
        
        protected override async Awaitable TransitionAsync() {
            await CameraHelpers.TransitionToViewAsync(_cam, _targetView, _duration);
        }
    }
}
