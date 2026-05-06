using UnityEngine;

namespace Metroma
{
    public class TestFloatProvider : MonoBehaviour
    {
        [ConditionParam] public float TimerRestant;
        private void Update()
        {
            TimerRestant += Time.deltaTime;
        }
    }
}
