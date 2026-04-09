using UnityEngine;

namespace Metroma
{
    public class ActivateGameObject : Sequencable
    {
        [SerializeField] private MonoBehaviour _object;
        [SerializeField] private bool _active;


        public override async Awaitable ExecuteAsync() {
            _object.enabled = _active;
        }
    }
}
