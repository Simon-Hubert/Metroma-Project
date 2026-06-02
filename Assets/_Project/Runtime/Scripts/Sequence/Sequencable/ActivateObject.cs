using UnityEngine;

namespace Metroma
{
    public class ActivateObject : ASequencable
    {
        [SerializeField] private GameObject _object;
        [SerializeField] private bool _active;
        
        public override async Awaitable ExecuteAsync() {
            if (!RequirementsValidated()) return;
            _object.SetActive(_active);
        }

        public override bool RequirementsValidated() {
            return _object;
        }
    }
}
