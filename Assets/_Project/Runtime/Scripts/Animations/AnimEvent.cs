using UnityEngine;
using UnityEngine.Events;

namespace Metroma
{
    public class AnimEvent : MonoBehaviour
    {
        [SerializeField] private UnityEvent _event;
        
        public void Invoke() {
            _event?.Invoke();
        }
    }
}
