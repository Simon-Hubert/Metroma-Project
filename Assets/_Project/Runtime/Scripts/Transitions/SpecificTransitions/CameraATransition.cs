using System;
using NaughtyAttributes;
using UnityEngine;

namespace Metroma.Transitions
{
    public class CameraATransition : ATransition {
        [Header("Camera references")]
        [SerializeField] private Camera _cam1;
        [SerializeField] private Camera _cam2;
    }
}
