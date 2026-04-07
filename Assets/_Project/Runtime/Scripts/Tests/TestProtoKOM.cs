using System;
using UnityEngine;

namespace Metroma
{
    public class TestProtoKOM : MonoBehaviour
    {
        [SerializeField] private TransitionManager _transManager;
        [SerializeField] private MiniGameTransitionData _data;

        [Header("MiniGameSide")] 
        [SerializeField] private Camera _miniGameCam;
        [SerializeField] private AView _defaultView;
        [SerializeField] private AView _targetView;

        private void Start() {
            _transManager.TryTransition(_data);
            _transManager.OnTransitionEnded += Next;
        }
        
        private void Next(){
            CameraHelpers.TransitionViewToView(_miniGameCam, _defaultView, _targetView, 2.5f);
            _transManager.OnTransitionEnded -= Next;
        }
    }
}
