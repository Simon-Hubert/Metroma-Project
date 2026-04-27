using UnityEngine;

namespace Metroma
{
    public class EnableBehaviour : ASequencable
    {
        [SerializeField] private MonoBehaviour behaviour;
        [SerializeField] private bool _enabled;
        
        public override async Awaitable ExecuteAsync() {
            behaviour.enabled = _enabled;
        }
    }
}
