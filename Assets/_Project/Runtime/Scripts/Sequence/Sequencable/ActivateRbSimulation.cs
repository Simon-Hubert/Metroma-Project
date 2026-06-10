using UnityEngine;

namespace Metroma
{
    public class ActivateRbSimulation : ASequencable
    {
        [SerializeField] private Rigidbody2D _object;
        [SerializeField] private bool _active;
        
        public override async Awaitable ExecuteAsync() {
            _object.simulated = _active;
        }
    }
}
