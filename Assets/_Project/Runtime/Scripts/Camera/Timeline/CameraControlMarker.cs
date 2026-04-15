using UnityEngine;
using UnityEngine.Timeline;

namespace Metroma.CameraTool.Timeline
{
    [CustomStyle("CameraControlMarker")]
    public class CameraControlMarker : CameraMarkerBase
    {
        [Tooltip("If checked, the CameraTool will automatically handle the camera. If unchecked, it releases control.")]
        public bool activeControl = false;

        [Tooltip("Optional action to perform on the Timeline director when this marker is hit.")]
        public DirectorAction directorAction = DirectorAction.None;

        public override void Execute(CameraTool tool)
        {
            if (tool == null)
            {
                return;
            }
            
            tool.SetControlActive(activeControl);

            // Handle Director Action
            if (tool.EditorDirector != null)
            {
                if (directorAction == DirectorAction.Pause)
                {
                    tool.EditorDirector.Pause();
                }
                else if (directorAction == DirectorAction.Stop)
                {
                    tool.EditorDirector.Stop();
                }
            }
        }
    }
}
