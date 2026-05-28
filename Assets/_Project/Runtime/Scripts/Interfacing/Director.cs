using System;
using System.Threading;
using Metroma.CameraTool;
using Metroma.Transitions;
using NaughtyAttributes;
using UnityEngine.Timeline;
using UnityEngine;
using UnityEngine.Playables;

namespace Metroma.Interfacing
{
    [Serializable]
    public struct PhaseData
    {
        public Camera AdCam;
        public ATransition TransiIn;
        public AdBase Ad;
        public ATransition TransiOut;
    }
    
    public class Director : MonoBehaviour
    {
        [SerializeField] private Camera _irlCam;
        [SerializeField] private FPSControllable _fpsControllable;
        [SerializeField] private PhaseData _phase1;
        [SerializeField] private PhaseData _phase2;

        [Header("PhaseSecific")]
        [SerializeField] private Animator _animMetro;
        [SerializeField] private Animator _animMec;
        [SerializeField, AnimatorParam("_animMetro")] private string _triggerAller;
        [SerializeField, AnimatorParam("_animMetro")] private string _triggerRetour;
        [SerializeField, AnimatorParam("_animMec")] private string _triggerMec;
        [SerializeField] private GameObject _irlAd1;
        [SerializeField] private GameObject _irlAd2;

        delegate void OnEndDelegate();
        
        private bool phase1Played;
        private bool phase2Played;
        private RenderTexture _temp;
        
        private void StartPhase(PhaseData phase, OnEndDelegate endDelegate = null) {
            _fpsControllable.enabled = false;
            phase.Ad.OnAdEnded += () =>
            {
                EndAdPhase(phase, endDelegate);
            };
            
            phase.TransiIn.OnTransitionEnd += () =>
            {
                _temp = phase.AdCam.targetTexture;
                phase.AdCam.targetTexture = null;
                _irlCam.enabled = false;
                phase.Ad.StartAd();
            };
            
            _ = phase.TransiIn.PlayAsync(CancellationToken.None);
        }

        private void EndAdPhase(PhaseData phase, OnEndDelegate endDelegate = null) {
            phase.AdCam.targetTexture = _temp;
            _irlCam.enabled = true;
            phase.TransiOut.OnTransitionEnd += () =>
            {
                _fpsControllable.enabled = true;
                _fpsControllable.Reset();
                if (endDelegate != null) endDelegate();
            };
            _ = phase.TransiOut.PlayAsync(CancellationToken.None);
            
        }

        public void Start() {
            _animMetro.SetTrigger(_triggerAller);
            _animMec.SetTrigger(_triggerMec);
        }

        private void InterPhase() {
            Debug.Log("InterPhase Started");
            _ = InterPhaseAsync();
        }

        private async Awaitable InterPhaseAsync() {
            _animMetro.SetTrigger(_triggerRetour);  
            await Awaitable.WaitForSecondsAsync(25f);
            _irlAd1.SetActive(false);
            _irlAd2.SetActive(true);
            _animMetro.SetTrigger(_triggerAller);
        }
        
        [Button]
        public void StartPhase1() {
            if (phase1Played) return;
            StartPhase(_phase1, InterPhase);
            phase1Played = true;
        }

        [Button]
        public void StartPhase2() {
            if (phase2Played) return;
            StartPhase(_phase2);
            phase2Played = true;
        }
        
    }
}