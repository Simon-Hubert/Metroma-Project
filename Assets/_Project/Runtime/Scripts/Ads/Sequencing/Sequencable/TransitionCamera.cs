using UnityEngine;

namespace Metroma
{
    public class TransitionCamera : Sequencable
    {
        [SerializeField] private AView _from;
        [SerializeField] private AView _to;
        [SerializeField] private Camera _camera;
        [SerializeField] private float _duration;
        [SerializeField] private bool _async;
        
        public override async Awaitable ExecuteAsync() {
            if (_async) {
                await CameraHelpers.ViewToViewTransitionAsync(_camera, _from, _to, _duration);
            }
            else {
                CameraHelpers.TransitionViewToView(_camera, _from, _to, _duration);
            }
        }
    }
}
