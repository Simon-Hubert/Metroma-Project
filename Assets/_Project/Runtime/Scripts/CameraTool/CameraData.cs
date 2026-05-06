using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using System.Collections.Generic;

namespace Metroma.CameraTool
{
    /// <summary>
    /// Defines a static camera view with position, rotation, FOV and distance offset.
    /// Optimized for memory alignment and fast interpolation.
    /// </summary>
    [System.Serializable]
    public struct CameraPose
    {
        public Vector3 position;
        public Quaternion rotation;
        public float fov;
        public float distance;
        public Vector3 up;

        public static CameraPose Identity => new CameraPose
        {
            position = Vector3.zero,
            rotation = Quaternion.identity,
            fov = 60f,
            distance = 0f,
            up = Vector3.up
        };

        public static CameraPose Lerp(CameraPose InA, CameraPose InB, float InT)
        {
            return new CameraPose
            {
                position = Vector3.LerpUnclamped(InA.position, InB.position, InT),
                rotation = Quaternion.SlerpUnclamped(InA.rotation, InB.rotation, InT),
                fov = Mathf.LerpUnclamped(InA.fov, InB.fov, InT),
                distance = Mathf.LerpUnclamped(InA.distance, InB.distance, InT),
                up = Vector3.SlerpUnclamped(InA.up, InB.up, InT)
            };
        }
    }

    /// <summary> Current state of the camera controller. </summary>
    public enum CameraState
    {
        TimelineDriven,  // Driven by FocusCam or other Timeline tracks
        Transitioning,   // Blending between states
        StaticPose,      // Holding on a fixed position
        Manual           // Under manual or FPS control
    }

    /// <summary> Action to perform on the Timeline director when reaching a transition end. </summary>
    public enum DirectorAction
    {
        None,
        Pause,
        Stop
    }

    /// <summary> Target for camera rotation during a transition. </summary>
    public enum TransitionLookAtMode
    {
        TimelineDefault,
        SpecificGameObject
    }
}
