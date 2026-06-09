using UnityEngine;

namespace Metroma
{
    public class PlaySoundSequence : ASequencable
    {
        [SerializeField] private AudioSource _source;
        [SerializeField] private bool _active = true;
        [Space(5)]
        [SerializeField] private bool _waitUntilFinishPlay = false;
        
        public override async Awaitable ExecuteAsync() {
            if (!_source) return;
            
            if (_active) _source.Play();
            else _source.Stop();
            
            if (!_source.loop && _waitUntilFinishPlay)
                await Awaitable.WaitForSecondsAsync(_source.clip.length);
        }
    }
}
