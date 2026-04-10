using UnityEngine;

namespace Metroma
{
    public class ActivateControllable : Sequencable
    {
        [SerializeField] private Controllable _controllable;
        [SerializeField] private bool _active;
        
        public override async Awaitable ExecuteAsync() {
            _controllable.IsActive = _active;
        }
    }
}
