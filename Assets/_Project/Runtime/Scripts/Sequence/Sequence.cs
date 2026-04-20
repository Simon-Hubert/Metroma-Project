using System;
using System.Collections.Generic;
using UnityEngine;

namespace Metroma
{
    public class Sequence : ASequencable
    {
        private List<ASequencable> _sequence = new List<ASequencable>();
        public event Action SequenceEnded;

        private void Start() {
            //TODO faire ce setup in editor
            foreach (Transform child in transform) {
                ASequencable sequencable = child.GetComponent<ASequencable>();
                if(sequencable) _sequence.Add(sequencable);
            }
        }

        public override async Awaitable ExecuteAsync() {
            try {
                foreach (ASequencable element in _sequence) {
                    Debug.Log($"{element.name} executing !");
                    await element.ExecuteAsync();
                }
            }
            catch (OperationCanceledException oce) {

            }
            finally {
                SequenceEnded?.Invoke();
            }
        }
    }
}
