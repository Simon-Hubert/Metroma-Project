using System;
using System.Collections.Generic;
using UnityEngine;

namespace Metroma
{
    public class Sequence : ASequencable
    {
        private List<ASequencable> _sequence = new List<ASequencable>();
        public event Action SequenceEnded;

        private void Awake() {
            //TODO faire ce setup in editor
            foreach (Transform child in transform) {
                ASequencable sequencable = child.GetComponent<ASequencable>();
                if(sequencable) _sequence.Add(sequencable);
            }
        }

        public void Execute() {
            _ = ExecuteAsync();
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
        
        
#if UNITY_EDITOR
        public override InspectorColor GetInspectorColor() {
            return new InspectorColor
            {
                Background = new Color(1f,0,0),
                Text = new Color(0.3f,0f,0f),
            };
        }
#endif
    }
}
