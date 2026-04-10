using UnityEngine;

namespace Metroma
{
    public class ConditionnalSequence : Sequence
    {
        [SerializeField] private ConditionalEvent _conditionalEvent;
    
        private void Awake() {
            _conditionalEvent.OnValidated += Execution;
        }

        private void Execution() {
            Debug.Log($"{name} started");
            ExecuteAsync();
        }
    }
}
