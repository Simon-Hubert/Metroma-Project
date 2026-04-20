using System;
using UnityEngine;

namespace Metroma
{
    public class CameraTransition : Transition
    {
        [Header("Camera references")]
        [SerializeField] private Camera _cam1;
        [SerializeField] private Camera _cam2;
        public override async Awaitable PlayAsync()
        {
            throw new NotImplementedException(); //TODO IMPLEMENT FUNCTION IN CAMERATRANSITION CLASS
        }
    }
}
