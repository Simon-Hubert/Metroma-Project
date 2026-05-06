using Metroma.CameraTool;
using UnityEngine.Timeline;
using System;
using System.ComponentModel;
using UnityEngine;

namespace Metroma.CameraTool.Timeline
{
    [Serializable]
    [DisplayName("Camera/🎬 Pose Transition")]
    [CustomStyle("CameraEventMarker")]
    public class CameraPoseMarker : CameraMarkerBase
    {
        [Header("Target Pose")]
        public Vector3 position;
        public Vector3 rotationEuler;
        public float fov = 60f;
        public float distance = 0f;

        public float duration = 1.0f;
        public AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        public DirectorAction action = DirectorAction.None;

        public override void Execute(CameraRig rig)
        {
            if (rig == null)
                return;

            CameraPose target = new CameraPose
            {
                position = this.position,
                rotation = Quaternion.Euler(rotationEuler),
                fov = this.fov,
                distance = this.distance
            };

            rig.TransitionToPose(target, duration, curve, action);
        }
    }
}
