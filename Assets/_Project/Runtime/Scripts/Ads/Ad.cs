using System;
using UnityEngine;

namespace Metroma
{
    public class Ad : MonoBehaviour
    {
        [SerializeField] private ConditionalEvent _winCondition;
        
        public event Action OnAdStarted;
        public event Action OnAdEnded;

        public virtual void StartAd() {
            OnAdStarted?.Invoke();
        }
        protected virtual void OnAdEnd() {
            OnAdEnded?.Invoke();
        }
    }
}
