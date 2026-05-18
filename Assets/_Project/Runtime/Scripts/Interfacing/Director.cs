using System.Threading;
using Metroma.CameraTool;
using Metroma.Transitions;
using UnityEngine.Timeline;
using UnityEngine;
using UnityEngine.Playables;

namespace Metroma.Interfacing
{
    public class Director : MonoBehaviour
    {
        [SerializeField] private PlayableDirector _director;
        [SerializeField] private TimelineAsset _timelineIntro;
        [SerializeField] private TimelineAsset _timeline11;
        [SerializeField] private ATransition _Phase11TransiIn;
        [SerializeField] private ATransition _Phase11TransiOut;
        [SerializeField] private AdBase _ad;
        [SerializeField] private Camera _adCam;
        [SerializeField] private Camera _irlCam;

        private RenderTexture _temp;
        
        private void Start() {
            _Phase11TransiIn.OnTransitionEnd += StartAd;
            CameraRig.Active.Sequences.PlayFocusStandalone(
                _director,
                _timelineIntro,
                0f,
                0f,
                false,
                null,
                () =>
                {
                    CameraRig.Active.SetControlActive(false);
                    _ = _Phase11TransiIn.PlayAsync(CancellationToken.None);
                }
                );
            
        }

        private void StartAd() {
            _ad.OnAdEnded += OnAdEnded;
            _temp = _adCam.targetTexture;
            _adCam.targetTexture = null;
            _irlCam.enabled = false;
            _ad.StartAd();
        }
        
        private void OnAdEnded() {
            _adCam.targetTexture = _temp;
            _irlCam.enabled = true;
            _Phase11TransiOut.OnTransitionEnd += () =>
            {
                CameraRig.Active.Sequences.PlayFocusStandalone(
                    _director,
                    _timeline11,
                    0f,
                    0f,
                    false
                );
            };
            _ = _Phase11TransiOut.PlayAsync(CancellationToken.None);
        }
    }
}