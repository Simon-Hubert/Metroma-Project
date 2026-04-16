using Metroma.CameraTool;
using UnityEngine.Timeline;
using System;
using System.ComponentModel;
using Metroma.CameraTool.Modifiers;

namespace Metroma.CameraTool.Timeline
{
    [Serializable]
    [DisplayName("Camera/🛑 Stop all modifiers")]
    [CustomStyle("CameraStopAllMarker")]
    public class CameraStopAllModifiersMarker : CameraMarkerBase
    {
        public override void Execute(CameraRig rig)
        {
            if (rig.TargetCamera != null)
            {
                CameraModifiers.StopAllCameraModifiers(rig.TargetCamera);
            }
        }
    }
}
