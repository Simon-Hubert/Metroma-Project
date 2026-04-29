using Metroma.Transitions;
using Shapes;
using System;
using System.Threading;
using UnityEngine;

namespace Metroma
{
    public class ColorShapeTransition : ATransition
    {
        [SerializeField] private ShapeRenderer _Shape;
        [SerializeField] private bool _useShapeColorAsStart;
        [SerializeField] private Color _fromColor;
        [SerializeField] private Color _targetColor;
        [SerializeField] private float _duration;
        
        protected override async Awaitable TransitionAsync(CancellationToken cancelToken) {
            try {
                float timer = 0;
                if (_useShapeColorAsStart && _Shape != null) _fromColor = _Shape.Color;

                while (timer < _duration) {
                    timer += Time.deltaTime;
                    _Shape.Color = Color.Lerp(_fromColor, _targetColor, timer / _duration);
                    await Awaitable.NextFrameAsync(cancelToken);
                }
            }
            catch (OperationCanceledException) {
                return;
            }
        }
    }
}
