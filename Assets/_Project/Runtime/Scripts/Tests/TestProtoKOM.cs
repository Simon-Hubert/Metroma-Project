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
        [SerializeField] private List<AView> _adsViews;

        private int _viewIndex = 0;

        private void Start() {
            if(_startAllMinigames) StartAllMiniGames();
            else StartMiniGame();
        }

        private void StartMiniGame() {
            _data.IsIn = true;
            _transManager.TryTransition(_data);
            CameraHelpers.TransitionViewToView(_miniGameCam, _defaultView, _adsViews[_viewIndex], 2.5f);
            _adsManager.StartAd();
            _adsManager.CurrentAd.OnAdEnded += OnAdEnded;
        }
        
        private void StartAllMiniGames() {
            _data.IsIn = true;
            _transManager.TryTransition(_data);
            CameraHelpers.TransitionViewToView(_miniGameCam, _defaultView, _adsViews[_viewIndex], 2.5f);
            _adsManager.StartAd();
        }

        private void OnAdEnded() {
            _adsManager.CurrentAd.OnAdEnded -= OnAdEnded;
            _data.IsIn = false;
            _transManager.TryTransition(_data);
            CameraHelpers.TransitionViewToView(_miniGameCam, _adsViews[_viewIndex], _defaultView, 2.5f);
        }

        public void NextAd()
        {
            _viewIndex++;
            CameraHelpers.TransitionViewToView(_miniGameCam, _adsViews[_viewIndex - 1], _adsViews[_viewIndex], 2.5f);
            _adsManager.NextAd();
        }
        
    }
}
