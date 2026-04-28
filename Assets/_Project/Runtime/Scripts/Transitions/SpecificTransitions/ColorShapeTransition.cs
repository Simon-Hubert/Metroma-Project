using Metroma.Transitions;
using Shapes;
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
        
        protected override async Awaitable TransitionAsync() {
            float timer = 0;
            if (_useShapeColorAsStart) _fromColor = _Shape.Color;
            
            while (timer < _duration) {
                timer += Time.deltaTime;
                _Shape.Color = Color.Lerp(_fromColor, _targetColor, timer / _duration);
                await Awaitable.NextFrameAsync();
            }
        }
    }
}
