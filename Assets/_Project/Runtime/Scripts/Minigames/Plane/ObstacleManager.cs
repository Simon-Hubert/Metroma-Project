using NaughtyAttributes;
using UnityEngine;

namespace Metroma
{
    public class ObstacleManager : MonoBehaviour
    {
        [SerializeField, MinMaxSlider(-10, 10)] private Vector2 _maxRange;
        [SerializeField] private float _interval;
        [SerializeField, MinMaxSlider(-1,1)] private float _intervalRange;
    }
}
