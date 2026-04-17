using System.ComponentModel;
using Metroma.CameraTool;
using UnityEngine.Timeline;
using UnityEngine;

namespace Metroma.CameraTool.Timeline
{
    public class CameraControlMarker : CameraMarkerBase
    {
        [Tooltip("If checked, the CameraTool will automatically handle the camera. If unchecked, it releases control.")]
        public bool activeControl = false;

        [Tooltip("Optional action to perform on the Timeline director when this marker is hit.")]
        public DirectorAction directorAction = DirectorAction.None;

        public override void Execute(CameraRig rig)
        {
            if (rig == null)
            {
                return;
            }
            
            rig.SetControlActive(activeControl);

            // Handle Director Action
            if (rig.EditorDirector != null)
            {
                if (directorAction == DirectorAction.Pause)
                {
                    rig.EditorDirector.Pause();
                }
                else if (directorAction == DirectorAction.Stop)
                {
                    rig.EditorDirector.Stop();
                }
            }
        }
    }
}
