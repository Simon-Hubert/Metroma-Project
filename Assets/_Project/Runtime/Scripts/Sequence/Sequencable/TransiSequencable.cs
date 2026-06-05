using System.Threading;
using Metroma.Transitions;
using UnityEngine;

namespace Metroma
{
    public class TransiSequencable : ASequencable
    {
        [SerializeField] private ATransition _trans;
        private readonly CancellationTokenSource _source = new CancellationTokenSource();
        [SerializeField] private bool _waitForEnd = true;
        
        public override async Awaitable ExecuteAsync() {
            if (_waitForEnd)
                await _trans.PlayAsync(_source.Token);
            else
                _ = _trans.PlayAsync(_source.Token);
        }

        public void Cancel() {
            _source.Cancel();
        }
    }
}
