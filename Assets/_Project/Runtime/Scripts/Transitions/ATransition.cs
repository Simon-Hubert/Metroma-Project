using UnityEngine;
using System;
using System.Threading;
using System.Threading.Tasks;
using NaughtyAttributes;

namespace Metroma
{
    public abstract class ATransition : MonoBehaviour {
        [Header("Transition Params")]
        [SerializeField] protected float _duration;
        protected float _currentTime;
        private CancellationToken _cancellationToken;
        
        [SerializeField, ReadOnly] protected bool isPlaying;
        public bool GetIsPlaying { get => isPlaying; }

        public virtual async Awaitable PlayAsync() {
            if (isPlaying)
                return;

            _cancellationToken = new CancellationToken();
            isPlaying = true;
            _currentTime = 0f;

            try
            {
                while (_currentTime < _duration) {
                    _currentTime += Time.deltaTime;
                    OnUpdate(Time.deltaTime);
                    await Awaitable.NextFrameAsync();
                }
            }
            catch
            {
                Debug.LogError($"Transition {name} : Task was cancelled");
                return;
            }
            finally
            {
                _cancellationToken.Dispose();;
                _cancellationToken = null;
                
                isPlaying = false;
            }
        }

        protected virtual void OnUpdate(float delta) => throw new NotImplementedException();
    }
}