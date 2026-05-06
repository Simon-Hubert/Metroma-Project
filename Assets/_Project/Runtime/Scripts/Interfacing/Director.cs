using System;
using System.Collections;
using Codice.CM.Common;
using Metroma.CameraTool;
using Metroma.CameraTool.Modules;
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
        
        private void Start() {
            CameraRig.Active.Sequences.PlayFocusStandalone(
                _director,
                _timelineIntro,
                0f,
                0f,
                false
                );
        }
    }
}