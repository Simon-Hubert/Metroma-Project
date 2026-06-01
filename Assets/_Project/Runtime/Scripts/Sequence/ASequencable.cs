using UnityEngine;

namespace Metroma
{
    public abstract class ASequencable : MonoBehaviour
    {
        public abstract Awaitable ExecuteAsync();

        public virtual bool RequirementsValidated() => true;
    }
}
