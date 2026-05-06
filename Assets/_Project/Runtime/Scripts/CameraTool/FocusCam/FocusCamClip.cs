using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using System.ComponentModel;

namespace Metroma.FocusCam
{
    public enum FocusMode
    {
        [InspectorName("🎯 Focus Point (Look At)")]
        LookAtPoint,
        
        [InspectorName("🧭 Fixed Direction (Rotation)")]
        KeepOrientation
    }

    [Serializable]
    public class FocusCamClip : PlayableAsset, ITimelineClipAsset
    {
        public FocusMode mode;
        
        [Tooltip("The world position the camera will look at (used in 'Focus Point' mode).")]
        public Vector3 position;
        
        [Tooltip("The fixed rotation the camera will adopt (used in 'Fixed Direction' mode).")]
        public Vector3 rotation;

        [Header("Camera Placement")]
        public bool overridePosition = false;
        public Vector3 cameraPosition;

        [Header("Optics & Effects")]
        public bool overrideFOV = false;
        
        [Range(10, 150)]
        public float fov = 60f;
        
        [Range(-180, 180)]
        public float roll = 0f;

        [Header("Editor Visuals")]
        public bool useCustomColor = false;
        public Color customColor = Color.white;

        public ClipCaps clipCaps => ClipCaps.Blending;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<FocusCamBehaviour>.Create(graph);
            FocusCamBehaviour behaviour = playable.GetBehaviour();

            behaviour.mode = mode;
            behaviour.position = position;
            behaviour.rotation = rotation;
            
            behaviour.overridePosition = overridePosition;
            behaviour.cameraPosition = cameraPosition;
            
            behaviour.overrideFOV = overrideFOV;
            behaviour.fov = fov;
            behaviour.roll = roll;

            return playable;
        }
    }

    [Serializable]
    public class FocusCamBehaviour : PlayableBehaviour
    {
        public FocusMode mode;
        public Vector3 position;
        public Vector3 rotation;
        
        public bool overridePosition;
        public Vector3 cameraPosition;
        
        public bool overrideFOV;
        public float fov;
        public float roll;
    }
}
