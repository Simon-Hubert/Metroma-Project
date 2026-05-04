using UnityEngine;

namespace Metroma
{
    public class WaitForDuration : ASequencable
    {
        [SerializeField] private float _duration;
        
        public override async Awaitable ExecuteAsync() {
            await Awaitable.WaitForSecondsAsync(_duration);
        }
    }
}
