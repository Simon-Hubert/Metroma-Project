using System;
using UnityEngine;

namespace Metroma
{
    public class AdSequenceManager : MonoBehaviour
    {
        [SerializeField] private CameraController _cameraController;
        
        [Header("ToothPaste")]
        [SerializeField] private AdManagerToothpaste _adManagerToothpaste;
        [SerializeField] private string _nextChapterToothpaste;
        [SerializeField] private Transform _adBillboard;
        [SerializeField] private Camera _cam;
        [SerializeField] private DollyView _view;

        [Header("Mains")]
        [SerializeField] private DollyView _handsView;
        [SerializeField] private TestProtoKOM _protoKomHands;
        
        [Header("Enzo")]
        [SerializeField] private DollyView _enzoView;
        [SerializeField] private TestProtoKOM _protoKomEnzo;

        private void Start() {
            StartDentifric();
        }
        
        public void StartDentifric() {
            _ = StartDentifricAsync();
        }

        private async Awaitable StartDentifricAsync() {
            await _view.AnimateView(5f);
            _cameraController.SetIsActive(false);
            await _adManagerToothpaste.StartMiniGame();
            _adBillboard.SetParent(_cam.transform, true);
            _adManagerToothpaste.OnEnded += StartMains;
        }
        
        public void StartMains() {
            _adManagerToothpaste.OnEnded -= StartMains;
            _ = StartMainsAsync();
        }

        private async Awaitable StartMainsAsync() {
            _cameraController.SetView(_handsView);
            _cameraController.SetIsActive(true);
            await Awaitable.WaitForSecondsAsync(0.1f);
            _cam.transform.DetachChildren();
            await _handsView.AnimateView(7.5f);
            _cameraController.SetIsActive(false);
            _protoKomHands.StartMiniGame();
            _protoKomHands.OnAdEndedEvent += StartParfum;
        }

        public void StartParfum() {
            _protoKomHands.OnAdEndedEvent -= StartParfum;
            _ = StartParfumAsync();
        }
        
        private async Awaitable StartParfumAsync() {
            await CameraHelpers.TransitionToViewAsync(_cam, _enzoView, 0.75f);
            _cameraController.SetView(_enzoView);
            _cameraController.SetIsActive(true);
            await Awaitable.WaitForSecondsAsync(0.1f);
            _cam.transform.DetachChildren();
            await _enzoView.AnimateView(7.5f);
            _cameraController.SetIsActive(false);
            _protoKomEnzo.StartAllMiniGames();
            //_protoKomEnzo.OnAdEndedEvent += StartParfum;
        }

    }
}
