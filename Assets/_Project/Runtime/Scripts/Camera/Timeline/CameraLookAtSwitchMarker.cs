using System;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Timeline;
using Metroma.CameraTool;

namespace Metroma.CameraTool.Timeline
{
    /// <summary>
    /// Timeline marker: switches the LookAt target to another Transform
    /// managed by the RailModule's target list.
    /// </summary>
    [Serializable]
    [DisplayName("Camera/🎯 LookAt Switch")]
    [CustomStyle("CameraLookAtSwitchMarker")]
    public class CameraLookAtSwitchMarker : CameraMarkerBase
    {
        [Tooltip("Index into Rig's lookAtTargets list. -1 = disable LookAt.")]
        [SerializeField] private int targetIndex;

        [Tooltip("Transition duration in seconds. 0 = instant.")]
        [Min(0f)]
        [SerializeField] private float transitionDuration = 0.5f;

        public int TargetIndex => targetIndex;
        public float TransitionDuration => transitionDuration;

        public override void Execute(CameraRig rig)
        {
            if (rig.Rails != null)
            {
                rig.Rails.HandleLookAtSwitchByIndex(targetIndex, transitionDuration);
            }
        }
    }
}
