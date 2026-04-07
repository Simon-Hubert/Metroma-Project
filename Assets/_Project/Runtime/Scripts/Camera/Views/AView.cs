using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AView : MonoBehaviour
{
    public abstract CameraConfiguration GetConfiguration();

    private void OnDrawGizmosSelected() {
        GetConfiguration().DrawGizmo(Color.green);
    }
}
