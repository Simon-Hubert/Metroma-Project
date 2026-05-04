using System;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public struct CameraConfiguration
{
    public float Yaw;
    public float Pitch;
    public float Roll;
    public Vector3 Pivot;
    public float Distance;
    public float Fov;

    public Quaternion GetRotation() {
        return Quaternion.Euler(Pitch, Yaw, Roll);
    }

    public Vector3 GetPosition() {
        return Pivot + GetRotation() * Vector3.back * Distance;
    }
    
    public static CameraConfiguration operator +(CameraConfiguration left, CameraConfiguration right) {
        return new CameraConfiguration()
        {
            Yaw = left.Yaw + right.Yaw,
            Pitch = left.Pitch + right.Pitch,
            Roll = left.Roll + right.Roll,
            Pivot = left.Pivot + right.Pivot,
            Distance = left.Distance + right.Distance,
            Fov = left.Fov + right.Fov
        };
    }
    
    public static CameraConfiguration operator -(CameraConfiguration left, CameraConfiguration right) {
        return new CameraConfiguration()
        {
            Yaw = left.Yaw - right.Yaw,
            Pitch = left.Pitch - right.Pitch,
            Roll = left.Roll - right.Roll,
            Pivot = left.Pivot - right.Pivot,
            Distance = left.Distance - right.Distance,
            Fov = left.Fov - right.Fov
        };
    }
    
    public static CameraConfiguration operator *(float left, CameraConfiguration right) {
        return new CameraConfiguration()
        {
            Yaw = left * right.Yaw,
            Pitch = left * right.Pitch,
            Roll = left * right.Roll,
            Pivot = left * right.Pivot,
            Distance = left * right.Distance,
            Fov = left * right.Fov
        };
    }
    
    public static CameraConfiguration operator *(CameraConfiguration left, float right) {
        return right * left;
    }
    
    public static CameraConfiguration operator /(CameraConfiguration left, float right) {
        if (right == 0) throw new ArgumentException("Divided by zero");
        return new CameraConfiguration()
        {
            Yaw = left.Yaw * (1f/right),
            Pitch = left.Pitch * (1f/right),
            Roll = left.Roll * (1f/right),
            Pivot = left.Pivot * (1f/right),
            Distance = left.Distance * (1f/right),
            Fov = left.Fov * (1f/right),
        };
    }
    
    public void DrawGizmo(Color color) {
        Gizmos.color = color;
        Gizmos.DrawSphere(Pivot, 0.25f);
        Gizmos.DrawLine(Pivot, GetPosition());
        Gizmos.matrix = Matrix4x4.TRS(GetPosition(), GetRotation(), Vector3.one);
        Gizmos.DrawFrustum(Vector3.zero, Fov, 10f, 0.8f, Camera.main.aspect);
    }
}
