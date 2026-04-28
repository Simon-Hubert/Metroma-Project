using Metroma.Transitions;
using Shapes;
using UnityEngine;

namespace Metroma
{
    public class ColorShapeTransition : ATransition
    {
        [SerializeField] private ShapeRenderer _Shape;
        [SerializeField] private Color _targetColor;
        [SerializeField] private float _duration;
        
        protected override Awaitable TransitionAsync() {
            float timer = 0;
            
            while (timer < _duration) {
                timer += Time.deltaTime;
                
                await Awaitable.FixedUpdateAsync();
            }
        }
    }
}
