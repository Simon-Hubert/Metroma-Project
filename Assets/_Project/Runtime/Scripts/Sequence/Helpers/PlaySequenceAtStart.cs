using UnityEngine;

namespace Metroma
{
    public class PlaySequenceAtStart : MonoBehaviour
    {
        [SerializeField] private Sequence _sequence;
        
        void Start() {
            _ = _sequence.ExecuteAsync();
        }
    }
}
