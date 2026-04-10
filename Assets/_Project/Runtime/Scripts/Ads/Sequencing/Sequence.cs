using System;
using System.Collections.Generic;
using UnityEngine;

namespace Metroma
{
    public abstract class Sequencable : MonoBehaviour
    {
        public abstract Awaitable ExecuteAsync();
    }
    
    public class Sequence : Sequencable
    {
        private List<Sequencable> _sequence = new List<Sequencable>();

        private void Start() {
            foreach (Transform child in transform) {
                Sequencable sequencable = child.GetComponent<Sequencable>();
                if(sequencable) _sequence.Add(sequencable);
            }
        }
        

        public event Action SequenceEnded;

        public override async Awaitable ExecuteAsync() {
            try {
                foreach (Sequencable element in _sequence) {
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
