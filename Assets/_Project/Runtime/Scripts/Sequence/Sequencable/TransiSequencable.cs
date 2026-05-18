using System.Threading;
using Metroma.Transitions;
using UnityEngine;

namespace Metroma
{
    public class TransiSequencable : ASequencable
    {
        [SerializeField] private ATransition _trans;
        private readonly CancellationTokenSource _source = new CancellationTokenSource();
        
        
        public override async Awaitable ExecuteAsync() {
            await _trans.PlayAsync(_source.Token);
        }

        public void Cancel() {
            _source.Cancel();
        }
    }
}
