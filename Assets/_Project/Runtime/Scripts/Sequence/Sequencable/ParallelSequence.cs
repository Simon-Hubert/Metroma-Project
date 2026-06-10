using System.Collections.Generic;
using UnityEngine;

namespace Metroma
{
    public class ParallelSequence : ASequencable
    {
        private readonly List<ASequencable> _sequence = new List<ASequencable>();
        [SerializeField] private float _duration;
        
        private void Awake() {
            foreach (Transform child in transform) {
                if (!child.gameObject.activeSelf) continue;
                ASequencable sequencable = child.GetComponent<ASequencable>();
                if(sequencable) _sequence.Add(sequencable);
            }
        }
        
        public override async Awaitable ExecuteAsync() {
            foreach (ASequencable sequencable in _sequence) {
                _ = sequencable.ExecuteAsync();
            }

            await Awaitable.WaitForSecondsAsync(_duration);
        }
    }
}
