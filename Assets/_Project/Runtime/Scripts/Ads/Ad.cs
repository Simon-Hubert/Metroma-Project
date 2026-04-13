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
            _winCondition.OnValidated += OnAdEnd;
        }
        protected virtual void OnAdEnd() {
            Debug.Log($"{name} Ended !");
            OnAdEnded?.Invoke();
        }
    }
}
