using System;
using System.Threading;
using Metroma.Transitions;
using UnityEngine;

namespace Metroma
{
    public class AnimCurveTransition : ATransition
    {
        [SerializeField] private AnimationCurve _curve;
        public float GetValue => _curve.Evaluate(_current / _duration);
        
        [SerializeField] private float _duration;
        private float _current = 0;
        [SerializeField] private bool _resetValueAtEnd = false;
        
        protected override async Awaitable TransitionAsync(CancellationToken cancelToken)
        {
            try {
                _current = 0;

                while (_current < _duration) {
                    _current += Time.deltaTime;
                    await Awaitable.NextFrameAsync(cancelToken);
                }
            }
            catch (OperationCanceledException) {

            }
            finally {
                _current = _resetValueAtEnd ? 0 : _duration;
            }
        }
    }
}