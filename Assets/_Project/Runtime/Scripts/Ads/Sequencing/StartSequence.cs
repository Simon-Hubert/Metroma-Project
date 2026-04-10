using UnityEngine;

namespace Metroma
{
    public class StartSequence : Sequence
    {
        [SerializeField] private Ad _ad;

        private void Awake() {
            _ad.OnAdStarted += () => ExecuteAsync();
        }
    }
}
