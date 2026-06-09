using System;
using UnityEngine;
using UnityEngine.UI;

namespace Metroma
{
    public class BlackScreen : MonoBehaviour
    {
        [SerializeField] private Image _image;
        private static BlackScreen _instance;

        private void Awake() {
            Debug.Log("AWAKE WAS CALLLED");
            if (!_instance) {
                _instance = this;
            }
            else {
                Destroy(this);
            }
            _instance.gameObject.SetActive(false);
            _image.enabled = true;
        }

        public static void Show() {
            _instance.gameObject.SetActive(true);
        }
        
        public static void Hide() {
            _instance.gameObject.SetActive(false);
        }
    }
}
