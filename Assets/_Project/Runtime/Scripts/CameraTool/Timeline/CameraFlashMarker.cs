using Metroma.CameraTool;
using UnityEngine.Timeline;
using System;
using System.ComponentModel;
using UnityEngine;
using Metroma.CameraTool.Modifiers;


namespace Metroma.CameraTool.Timeline
{
    /// <summary>
    /// Timeline marker: triggers a momentary screen flash.
    /// Demonstrates the simplicity of the new Marker system.
    /// </summary>
    [Serializable]
    [DisplayName("Camera/🎬 Screen Flash")]
    public class CameraFlashMarker : CameraMarkerBase
    {
        [SerializeField] private Color flashColor = Color.white;
        [SerializeField] private float duration = 0.5f;

        public override void Execute(CameraRig rig)
        {
            if (rig.TargetCamera != null)
            {
                CameraModifiers.DoFlash(rig.TargetCamera, flashColor, duration);
            }
        }
    }
}
