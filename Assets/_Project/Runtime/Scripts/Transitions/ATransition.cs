using UnityEngine;
using System;
using Unity.Collections;

namespace Metroma
{
    public abstract class ATransition : MonoBehaviour {
        [Header("Transition Params")]
        [SerializeField] protected float _duration;
        protected float _currentTime;
        
        [SerializeField, ReadOnly] protected bool isPlaying;
        public bool GetIsPlaying { get => isPlaying; }

        public virtual async Awaitable PlayAsync() {
            if (isPlaying)
                return;

            isPlaying = true;
            _currentTime = 0f;

            while (_currentTime < _duration) {
                _currentTime += Time.deltaTime;
                OnUpdate(Time.deltaTime);
                await Awaitable.NextFrameAsync();
            }

            isPlaying = false;
        }

        protected virtual void OnUpdate(float delta) => throw new NotImplementedException();
    }
}