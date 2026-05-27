using UnityEngine;
using UnityEngine.Events;

namespace Metroma
{
    public class UnityEventSequence : ASequencable
    {
        [SerializeField] private UnityEvent _event;
        

        public override async Awaitable ExecuteAsync() {
            _event?.Invoke();
        }
    }
}
