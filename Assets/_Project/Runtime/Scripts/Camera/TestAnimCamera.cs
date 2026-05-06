using Metroma.CameraTool;
using Metroma.CameraTool.Modifiers;
using Metroma.CameraTool.Timeline;
using UnityEngine;

public class TestAnimCamera : MonoBehaviour
{
    [SerializeField] private CameraRig cameraRig;
    [SerializeField] private Camera cameraRef;
    
    void OnEnable()
    {
        if (cameraRig)
        {
            cameraRig.OnMarkerEventHit += HandleMarker;
        }
    }

    void OnDisable()
    {
        if (cameraRig)
        {
            cameraRig.OnMarkerEventHit -= HandleMarker;
        }
    }

    private void HandleMarker(CameraMarkerBase marker)
    {
        // Check for specific markers or use naming conventions
        if (marker is CameraEventMarker eventMarker)
        {
            string eventName = eventMarker.EventName;
            Debug.Log($"[TestAnimCamera] Received event: {eventName}");

            if (eventName == "Flash")
            {
                if (cameraRef != null)
                {
                    // Assuming DoChromaticAberration is an extension or part of a modifier
                    // cameraRef.DoChromaticAberration(100f, 5f);
                }
            }
        }
    }
}
