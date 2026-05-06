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
        [Tooltip("Enable or disable real-time Depth of Field sync.")]
        public bool active = true;

        [Tooltip("Optional: Specific target to focus on. If empty, it will use whatever target is currently set or remain inactive.")]
        public ExposedReference<Transform> focusTarget;

        public override void Execute(CameraRig rig, IExposedPropertyTable resolver)
        {
            if (rig.TargetCamera != null)
            {
                Transform target = focusTarget.Resolve(resolver);
                CameraModifiers.SetAutoFocus(rig.TargetCamera, active, target);
            }
        }
    }
}
