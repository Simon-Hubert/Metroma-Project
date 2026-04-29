using Metroma.CameraTool;
using UnityEngine.Timeline;
using System;
using System.ComponentModel;
using UnityEngine;

namespace Metroma.CameraTool.Timeline
{
    /// <summary>
    /// Timeline marker: switches the active rail in the CameraRig's spline chain.
    /// </summary>
    [Serializable]
    [DisplayName("Camera/🔀 Rail Switch")]
    [CustomStyle("CameraRailSwitchMarker")]
    public class CameraRailSwitchMarker : CameraMarkerBase
    {
        [Tooltip("Index of the rail to switch to in the spline list.")]
        [Min(0)]
        [SerializeField] private int railIndex;

        [Tooltip("If true, resets to full chain mode.")]
        [SerializeField] private bool resetToChainMode;

        public int RailIndex => railIndex;
        public bool ResetToChainMode => resetToChainMode;

        public override void Execute(CameraRig rig)
        {
            if (resetToChainMode)
            {
                rig.Rails.ResetToChainMode();
            }
            else
            {
                rig.Rails.SwitchToRail(railIndex);
            }
        }
    }
}
