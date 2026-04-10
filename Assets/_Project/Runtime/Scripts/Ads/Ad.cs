using System;
using NaughtyAttributes;
using UnityEngine;

namespace Metroma
{
    public class Ad : MonoBehaviour
    {
        [SerializeField] private ConditionalEvent _winCondition;
        
        public event Action OnAdStarted;
        public event Action OnAdEnded;
        
        [Button]
        public virtual void StartAd() {
            Debug.Log($"{name} started !");
            OnAdStarted?.Invoke();
        }
        protected virtual void OnAdEnd() {
            OnAdEnded?.Invoke();
        }
    }
}
