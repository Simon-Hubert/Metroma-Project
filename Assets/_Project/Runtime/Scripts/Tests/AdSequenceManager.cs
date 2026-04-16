using UnityEngine;

namespace Metroma
{
    public class AdSequenceManager : MonoBehaviour
    {
        [Header("ToothPaste")]
        [SerializeField] private AdManagerToothpaste _adManagerToothpaste;
        [SerializeField] private string _nextChapterToothpaste;
        [SerializeField] private Transform _adBillboard;
        [SerializeField] private Camera _cam;
        
        public void StartMains() {
            
        }

        public void StartParfum() {
            
        }

        public void StartDentifric() {
            _ = StartDentifricAsync();
        }

        private async Awaitable StartDentifricAsync() {
            CameraTool.CameraTool.Active.SetControlActive(false);
            await _adManagerToothpaste.StartMiniGame();
            _adBillboard.SetParent(_cam.transform, true);
            _adManagerToothpaste.OnEnded += () =>
            {
                _cam.transform.DetachChildren();
                CameraTool.CameraTool.Active.SetControlActive(true);
                CameraTool.CameraTool.Active.PlayChapter(_nextChapterToothpaste);
            };
        }
    }
}
