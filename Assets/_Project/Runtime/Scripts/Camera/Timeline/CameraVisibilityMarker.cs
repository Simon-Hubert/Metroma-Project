using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Metroma.CameraTool.Timeline
{
    /// <summary>
    /// Marker that checks if a specific scene object is entirely within the camera's view 
    /// for a specified duration.
    /// </summary>
    public class CameraVisibilityMarker : Marker, INotification
    {
        [Header("Targeting")]
        public ExposedReference<GameObject> targetObject;
        
        [Header("Conditions")]
        [Tooltip("How many consecutive seconds the object must be fully in view.")]
        public float requiredSeconds = 1.0f;
        
        [Tooltip("The ID of the event to trigger in the SequenceModule.")]
        public string eventId = "object_seen";

        [Header("Options")]
        public bool checkOcclusion = true;
        [Tooltip("If true, the check continues even after the marker has passed (until condition met).")]
        public bool persistUntilMet = false;

        public PropertyName id => new PropertyName("CameraVisibilityMarker");
    }
}
