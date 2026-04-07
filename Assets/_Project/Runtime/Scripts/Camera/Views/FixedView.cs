using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixedView : AView
{
    [SerializeField] private float _yaw;
    [SerializeField] private float _pitch;
    [SerializeField] private float _roll;
    [SerializeField] private float _fov;

    public override CameraConfiguration GetConfiguration() {
        CameraConfiguration config = new CameraConfiguration()
        {
            Pivot = transform.position,
            Distance = 0,
            Yaw = _yaw,
            Pitch = _pitch,
            Roll = _roll,
            Fov = _fov
        };
        return config;
    }
}