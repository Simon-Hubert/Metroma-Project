using UnityEngine;

namespace Metroma
{
    public class Timer : MonoBehaviour
    {
        [ConditionParam] private float _time;
        
        void Update()
        {
            _time += Time.deltaTime;
        }
    }
}
