using System;
using System.ComponentModel;
using UnityEngine;
using Metroma.CameraTool.Modifiers;


namespace Metroma.CameraTool.Timeline
{
    /// <summary>
    /// Timeline marker: Activates a handheld camera profile.
    /// </summary>
    [Serializable]
    [DisplayName("Camera/🎥 Handheld Mode")]
    public class CameraHandheldMarker : CameraMarkerBase
    {
        [Tooltip("Handheld profile to apply. If null, handheld mode is deactivated.")]
        [SerializeField] private CameraHandheldProfile profile;

        [Tooltip("If true, this marker deactivates handheld mode instead of activating a profile.")]
        [SerializeField] private bool deactivate = false;

        public override void Execute(CameraTool tool)
        {
            if (tool == null || tool.TargetCamera == null)
                return;

            if (deactivate)
            {
                CameraModifiers.SetHandheld(tool.TargetCamera, false, null);
            }
            else if (profile != null)
            {
                CameraModifiers.SetHandheld(tool.TargetCamera, true, profile);
            }
        }
    }
}
