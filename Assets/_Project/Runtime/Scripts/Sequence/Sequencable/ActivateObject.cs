using UnityEngine;

namespace Metroma
{
    public class ActivateObject : ASequencable
    {
        [SerializeField] private GameObject _object;
        [SerializeField] private bool _active;
        
        public override async Awaitable ExecuteAsync() {
            _object.SetActive(_active);
        }
    }
}
