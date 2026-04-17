using System;
using System.Collections.Generic;
using UnityEngine;

namespace Metroma
{
    public class TestProtoKOM : MonoBehaviour
    {
        [SerializeField] private bool _startAllMinigames = false;
        [SerializeField] private TransitionManager _transManager;
        [SerializeField] private MiniGameTransitionData _data;

        [Header("MiniGameSide")] 
        [SerializeField] private AdsManagerKOM _adsManager;
        [SerializeField] private Camera _miniGameCam;
        [SerializeField] private AView _defaultView;
        [SerializeField] private AView _endView;
        [SerializeField] private List<AView> _adsViews;

        public event Action OnAdEndedEvent;

        private int _viewIndex = 0;
        
        public void StartMiniGame() {
            _data.IsIn = true;
            _transManager.TryTransitionAsync(_data);
            CameraHelpers.TransitionToViewAsync(_miniGameCam, _adsViews[_viewIndex], 2.5f);
            _adsManager.StartAd();
            _adsManager.CurrentAd.OnAdEnded += OnAdEnded;
        }
        
        public void StartAllMiniGames() {
            _data.IsIn = true;
            _transManager.TryTransition(_data);
            CameraHelpers.TransitionViewToView(_miniGameCam, _defaultView, _adsViews[_viewIndex], 2.5f);
            _adsManager.StartAd();
            _adsManager.LastAd.OnAdEnded += OnAdEnded;
        }

        private void OnAdEnded() {
            _adsManager.CurrentAd.OnAdEnded -= OnAdEnded;
            _data.IsIn = false;
            _transManager.TryTransition(_data);
            CameraHelpers.TransitionViewToView(_miniGameCam, _adsViews[_viewIndex], _endView, 2.5f);
            OnAdEndedEvent?.Invoke();
        }

        public void NextAd(bool nextView)
        {
            if (nextView &&_viewIndex < _adsViews.Count - 1)
            {
                _viewIndex++;
                CameraHelpers.TransitionViewToView(_miniGameCam, _adsViews[_viewIndex - 1], _adsViews[_viewIndex], 3.5f);
            }
            _adsManager.NextAd();
        }
        
    }
}
