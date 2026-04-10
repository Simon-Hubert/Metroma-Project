using UnityEngine;

namespace Metroma
{
    public class ActivateBehaviour : Sequencable
    {
        [SerializeField] private GameObject _object;
        [SerializeField] private bool _active;


        public override async Awaitable ExecuteAsync() {
            _object.SetActive(_active);
            Debug.Log($"Set {_object} to {_active}");
        }
    }
}
