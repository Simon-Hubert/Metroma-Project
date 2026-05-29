using System;
using System.Threading;
using Metroma.Transitions;
using UnityEngine;

namespace Metroma
{
    public class ChangeSpriteTransition : ATransition
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Sprite _toSprite;
        
        protected override async Awaitable TransitionAsync(CancellationToken cancelToken)
        {
            try {
                if (_spriteRenderer) {
                    _spriteRenderer.sprite = _toSprite;
                }
            }
            catch (OperationCanceledException) {
                
            }

            return;
        }
    }
}