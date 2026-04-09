using UnityEngine;

namespace Metroma
{
    public class ConditionnalSequence : Sequence
    {
        [SerializeField] private ConditionalEvent _conditionalEvent;

        private void Start() {
            _conditionalEvent.OnValidated += () => ExecuteAsync();
        }
    }
}
