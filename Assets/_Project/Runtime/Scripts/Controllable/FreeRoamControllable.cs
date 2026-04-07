using UnityEngine;
using System;

namespace Metroma
{
    public class FreeRoamControllable : Controllable
    {
        [SerializeField] protected float _accelerationTime = 0.5f;
        [SerializeField] protected float _decelerationTime = 1.5f;

        [SerializeField] protected float _maxSpeed = 100.0f;
        [Tooltip("0 is no rotation speed")]
        [SerializeField] protected float _angleSpeed = 0f;
        
        protected void FixedUpdate()
        {
            
        }
    }
}
