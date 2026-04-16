using System;
using NaughtyAttributes;
using UnityEngine;

namespace Metroma
{
    public class AdManagerToothpaste : MonoBehaviour
    {
        [SerializeField] private TransitionManager _transManager;
        [SerializeField] private MiniGameTransitionData _data;

        [Header("MiniGameSide")] 
        [SerializeField] private Camera _miniGameCam;
        [SerializeField] private AView _defaultView;
        [SerializeField] private AView _targetView;
        [SerializeField] private AView _defaultView2;
        [SerializeField] private AView _targetView2;
        [SerializeField] private Ad _ad;

        public event Action OnEnded;
        
        [Button]
        public async Awaitable StartMiniGame() {
            _data.IsIn = true;
            await _transManager.TryTransitionAsync(_data);
            CameraHelpers.TransitionViewToView(_miniGameCam, _defaultView, _targetView, 2.5f);
            _ad.StartAd();
            _ad.OnAdEnded += OnAdEnded;
        }

        private void OnAdEnded() {
            _ = OnAdEndedAsync();
        }
        
        private async Awaitable OnAdEndedAsync() {
            _ad.OnAdEnded -= OnAdEnded;
            _data.IsIn = false;
            await _transManager.TryTransitionAsync(_data);
            CameraHelpers.TransitionViewToView(_miniGameCam, _defaultView2, _targetView2, 2.5f);
            OnEnded?.Invoke();
        }

    }
}
