using System;
using System.Threading;
using Dreamteck.Splines;
using Metroma.Transitions;
using UnityEngine;

namespace Metroma
{
    public class SplineTransition : ATransition
    {
        [SerializeField] private GameObject _object;
        [SerializeField] private float _duration;
        [SerializeField] SplineComputer _spline;
        
        protected override async Awaitable TransitionAsync(CancellationToken token) {
            try {
                float t = 0f;
                while (t < _duration) {
                    await Awaitable.NextFrameAsync();
                    t += Time.deltaTime;
                    float p = t / _duration;
                    _object.transform.position = _spline.EvaluatePosition(p);
                }
            }
            catch (OperationCanceledException oce) {
                
            }
        }
    }
}
