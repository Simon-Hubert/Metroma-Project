using UnityEngine;
using NaughtyAttributes;


namespace Metroma.Audio
{
    public class AudioLoopCall : MonoBehaviour
    {
        [SerializeField] private AK.Wwise.Event _playEvent;
        [SerializeField] private AK.Wwise.Event _stopEvent;
        [SerializeField] private GameObject _audioSource;

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
    }
}
