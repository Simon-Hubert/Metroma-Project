using UnityEngine;
using System;
using System.Threading;
using NaughtyAttributes;

namespace Metroma.Transitions
{
    public abstract class ATransition : MonoBehaviour {
        [Header("Transition Params")]
        [SerializeField] protected float duration;
        protected float currentTime;
        
        protected CancellationTokenSource cancelTokenSource;
        
        [SerializeField, ReadOnly] protected bool isPlaying;
        public bool GetIsPlaying { get => isPlaying; }

        public void Play()
        {
            cancelTokenSource?.Cancel();
            cancelTokenSource?.Dispose();
            cancelTokenSource = new CancellationTokenSource();
            _ = PlayAsync(cancelTokenSource.Token);
        }
        public virtual async Awaitable PlayAsync(CancellationToken cancelToken) {
            if (isPlaying)
                return;
            
            isPlaying = true;
            currentTime = 0f;

            try {
                while (currentTime < duration) {
                    currentTime += Time.deltaTime;
                    OnUpdate(Time.deltaTime);
                    await Awaitable.NextFrameAsync(cancelToken);
                }
            }
            catch(OperationCanceledException) {
                Debug.LogWarning($"Transition : Task was cancelled");
            }
            finally {
                isPlaying = false;
            }
        }

        protected virtual void OnUpdate(float delta) { }

        private void OnDestroy() {
            cancelTokenSource?.Cancel();
            cancelTokenSource?.Dispose();
        }
    }
}