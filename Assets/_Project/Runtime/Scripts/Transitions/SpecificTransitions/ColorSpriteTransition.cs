using Metroma.Transitions;
using System;
using System.Threading;
using UnityEngine;

namespace Metroma
{
    public class ColorSpriteTransition : ATransition
    {
        [SerializeField] private SpriteRenderer _Sprite;
        [SerializeField] private bool _useShapeColorAsStart = true;
        [SerializeField] private Color _fromColor;
        [SerializeField] private Color _targetColor;
        [SerializeField] private float _duration;
        
        protected override async Awaitable TransitionAsync(CancellationToken cancelToken) {
            try {
                float timer = 0;
                if (_useShapeColorAsStart && _Sprite != null) _fromColor = _Sprite.color;

                while (timer < _duration) {
                    timer += Time.deltaTime;
                    _Sprite.color = Color.Lerp(_fromColor, _targetColor, timer / _duration);
                    await Awaitable.NextFrameAsync(cancelToken);
                }
            }
            catch (OperationCanceledException) {
                
            }
            finally {
                _Sprite.color = _targetColor;
            }
        }
    }
}
