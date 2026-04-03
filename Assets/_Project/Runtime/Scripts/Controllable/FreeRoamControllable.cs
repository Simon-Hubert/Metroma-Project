using UnityEngine;
using System;

namespace Metroma
{
    public class FreeRoamControllable : Controllable
    {
        [SerializeField] private float _accelerationTime = 0.5f;
        [SerializeField] private float _decelerationTime = 1.5f;

        [Tooltip("0 is no rotation speed")]
        [SerializeField] private float _angleSpeed = 0f;
        
        protected void FixedUpdate()
        {
            
        }
    }
}
