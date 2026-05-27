using UnityEngine;

namespace Metroma
{

    public interface IInitiable
    {
        public void Init();
    }   
    
    public class BatchInitSequence : ASequencable
    {
        [SerializeField] private GameObject[] _initiables;


        public override async Awaitable ExecuteAsync() {
            foreach (GameObject initObject in _initiables) {
                foreach (MonoBehaviour component in initObject.GetComponents<MonoBehaviour>()) {
                    IInitiable init = component as IInitiable;
                    if (init != null) {
                        init.Init();
                    }
                }
            }
        }
    }
}
