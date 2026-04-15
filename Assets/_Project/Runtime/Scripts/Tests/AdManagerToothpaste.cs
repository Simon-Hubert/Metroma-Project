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
        public void StartMiniGame() {
            _data.IsIn = true;
            _transManager.TryTransition(_data);
            CameraHelpers.TransitionViewToView(_miniGameCam, _defaultView, _targetView, 2.5f);
            _ad.StartAd();
            _ad.OnAdEnded += OnAdEnded;
        }

        private void OnAdEnded() {
            _ad.OnAdEnded -= OnAdEnded;
            _data.IsIn = false;
            _transManager.TryTransition(_data);
            CameraHelpers.TransitionViewToView(_miniGameCam, _defaultView2, _targetView2, 2.5f);
            OnEnded?.Invoke();
        }

    }
}
