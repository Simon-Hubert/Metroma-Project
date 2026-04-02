using UnityEngine;
using NaughtyAttributes;

namespace Metroma
{
    public class InputManager : MonoBehaviour
    {
        public static InputManager instance;

        private void Awake()
        {
            if (instance != null)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
        }
        
        
    }
}
