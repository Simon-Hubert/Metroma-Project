using UnityEngine;

namespace Metroma
{
    public class Transition : MonoBehaviour
    {
        [Header("Transition Params")]
        [SerializeField] private float _duration;
        private float _currentTime;
        private bool _isPlaying;

        public virtual async Awaitable PlayAsync()
        {
            if (_isPlaying)
                return;

            _isPlaying = true;
            _currentTime = 0f;

            while (_currentTime < _duration)
            {
                _currentTime += Time.deltaTime;
                await Awaitable.NextFrameAsync();
            }

            _isPlaying = false;
        }
    }
}