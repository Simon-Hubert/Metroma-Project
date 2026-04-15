using UnityEngine;

namespace Metroma
{
    public class AdSequenceManager : MonoBehaviour
    {
        [SerializeField] private AdManagerToothpaste _adManagerToothpaste;
        [SerializeField] private Transform _adBillboard;
        [SerializeField] private Camera _cam;
        
        public void StartMains() {
            
        }

        public void StartParfum() {
            
        }

        public void StartDentifric() {
            _adManagerToothpaste.StartMiniGame();
            _adBillboard.SetParent(_cam.transform, true);
            _adManagerToothpaste.OnEnded += () => _cam.transform.DetachChildren();
        }
    }
}
