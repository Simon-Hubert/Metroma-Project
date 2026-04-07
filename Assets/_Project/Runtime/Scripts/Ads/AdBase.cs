using System;
using UnityEngine;

namespace Metroma
{

    public abstract class AdBase : MonoBehaviour
    {
        [SerializeField] protected Controllable[] controllables;
        
        public event Action OnAdStarted;
        public event Action OnAdEnded;

        public abstract void StartAd();
        protected virtual void OnAdEnd() {
            OnAdEnded?.Invoke();
        }
    }
}
