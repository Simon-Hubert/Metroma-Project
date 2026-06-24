using UnityEngine;
using NaughtyAttributes;


namespace Metroma.Audio
{
    public class AudioOneShotCall : MonoBehaviour
    {
        [SerializeField] private AK.Wwise.Event _playEvent;
        [SerializeField] private GameObject _audioSource;

        private void Start() {
            if (!_audioSource) _audioSource = Camera.main.gameObject;
        }
        
        [Button("Play Clip")]
        public void PlayAudio() {
            Debug.Log($"SOUND : Call sound {name}");
            _playEvent?.Post(_audioSource);
        }
    }
}
