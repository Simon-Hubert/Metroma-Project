using System;
using UnityEngine;
using NaughtyAttributes;
using System.Collections.Generic;

namespace Metroma.Audio
{
    public class MusicCall : MonoBehaviour
    {
        [SerializeField] private AK.Wwise.Event _playEvent;
        [SerializeField] private AK.Wwise.Event _stopEvent;
        [SerializeField] private GameObject _audioSource;
        
        [SerializeField] private List<AK.Wwise.State> _statesMusic;

        private void Start() {
            if (!_audioSource) _audioSource = Camera.main.gameObject;
        }

        [Button("Play Loop")]
        public void PlayAudio() {
            _playEvent?.Post(_audioSource);
        }
        [Button("Stop Loop")]
        public void StopAudio() {
            _stopEvent?.Post(_audioSource);
        }
        
        public void ChangeState(int index) {
            AK.Wwise.State state = _statesMusic[index];
            AkUnitySoundEngine.SetState(state.GroupId, state.Id);
        }

        public void PlayWithState(int index) {
            ChangeState(index);
            PlayAudio();
        }
    }
}
