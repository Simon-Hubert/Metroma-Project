using Metroma.CameraTool;
using UnityEngine.Timeline;
using System;
using System.ComponentModel;
using UnityEngine;
using Metroma.CameraTool.Modifiers;

namespace Metroma.CameraTool.Timeline
{
    [Serializable]
    [DisplayName("Camera/🎯 Auto-Focus Toggle")]
    public class CameraAutoFocusMarker : CameraMarkerBase
    {
        [Tooltip("Enable or disable real-time Depth of Field sync to the current target.")]
        public bool active = true;

        public override void Execute(CameraRig rig)
        {
            if (rig.TargetCamera != null)
            {
                CameraModifiers.SetAutoFocus(rig.TargetCamera, active, rig.CurrentLookAtTarget);
            }
        }
    }
}
