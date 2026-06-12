using UnityEngine;

namespace Metroma
{
    public class PlaySequenceAtStart : MonoBehaviour
    {
        [SerializeField] private Sequence _sequence;
        
        void Start() {
            _ = StartAsync();
        }

        async Awaitable StartAsync() {
            await Awaitable.NextFrameAsync(); 
            _sequence.Execute();
        }
    }
}
