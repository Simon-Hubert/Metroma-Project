using System;
using Metroma.Transitions;
using NaughtyAttributes;
using UnityEngine;

namespace Metroma
{
    public class CameraColorTransition : ATransition
    {
        [SerializeField] private Camera _cam;
        [SerializeField] private float _duration;
        [SerializeField] private bool _useGradient;
        [SerializeField, ShowIf("_useGradient")] private Gradient _gradient;
        [SerializeField, HideIf("_useGradient")] private Color _toColor;
        [SerializeField] private AnimationCurve _curve;

        protected override async Awaitable TransitionAsync() {
            float t = 0;
            Color startCol = _cam.backgroundColor;
            while (t < _duration) {
                Debug.Log(t);
                t += Time.deltaTime;
                float p = t / _duration;    
                p = _curve.Evaluate(p);
                Color col;
                if (_useGradient) {
                    col = _gradient.Evaluate(p);
                }
                else {
                    col = Color.Lerp(startCol, _toColor, p);
                }
                _cam.backgroundColor = col;
                await Awaitable.NextFrameAsync();
            }

            Color color;
            if (_useGradient) {
                color = _gradient.Evaluate(1);
            }
            else {
                color = Color.Lerp(startCol, _toColor, 1);
            }

            _cam.backgroundColor = color;
        }
    }
}
