using UnityEngine;
using UnityEditor;
using Dreamteck.Splines;
using System.Collections.Generic;


namespace Metroma.CameraTool.Editor
{
    /// <summary>
    /// Scene View gizmos for <see cref="CameraRig"/>:
    /// Per-segment colored spline path, node labels, progress indicator,
    /// camera frustum, and LookAt target line.
    /// </summary>
    public static class CameraToolGizmos
    {
        private static readonly Color ProgressSphereColor = new Color(0.1f, 0.9f, 0.4f, 0.9f);
        private static readonly Color DirectionColor = new Color(1f, 0.6f, 0.1f, 0.8f);
        private static readonly Color FrustumColor = new Color(0.2f, 0.6f, 1f, 0.8f);
        private static readonly Color LookAtLineColor = new Color(1f, 0.3f, 0.3f, 0.7f);
        private static readonly Color NodeLabelColor = new Color(0.9f, 0.9f, 0.9f, 0.9f);
        private static readonly Color SeparatorColor = new Color(1f, 1f, 1f, 0.5f);

        private const int SAMPLES_PER_SEGMENT = 16;
        private const float SPHERE_RADIUS = 0.25f;
        private const float PATH_DOT_RADIUS = 0.1f;
        private const float NODE_SPHERE_RADIUS = 0.4f;
        private const float DIRECTION_ARROW_LENGTH = 1.8f;
        
        private const float MIN_GIZMO_SCALE = 0.5f;
        private const float MAX_CAMERA_GIZMO_SCALE = 5.0f;
        private const float MAX_GIZMO_SCALE = 10.0f;

        [DrawGizmo(GizmoType.Selected | GizmoType.Active)]
        private static void DrawGizmos(CameraRig rig, GizmoType gizmoType)
        {
            if (rig == null || rig.Rails == null)
                return;

            List<SplineComputer> rails = rig.Rails.EditorRails;
            if (rails == null || rails.Count == 0)
                return;

            DrawSegmentedSplinePath(rig, rails);
            
            Vector3 targetPos = rig.TargetCamera != null ? rig.TargetCamera.transform.position : rig.transform.position;
            float targetScale = GetScale(targetPos, MAX_CAMERA_GIZMO_SCALE);

            DrawProgressPoint(rig, targetScale);
            DrawCameraFrustum(rig, targetScale);
            DrawLookAtLine(rig, targetScale);
            DrawProgressLabel(rig, targetScale);
        }

        private static float GetScale(Vector3 position, float max)
        {
            float rawScale = HandleUtility.GetHandleSize(position);
            return Mathf.Clamp(rawScale, MIN_GIZMO_SCALE, max);
        }
        
        // --- Per-Segment Colored Path ---
        private static void DrawSegmentedSplinePath(CameraRig rig, List<SplineComputer> rails)
        {
            int globalSegIndex = 0;
            Color mainColor = Color.cyan;

            for (int r = 0; r < rails.Count; r++)
            {
                SplineComputer spline = rails[r];
                if (spline == null)
                    continue;

                int nodeCount = spline.pointCount;
                int splineSegments = Mathf.Max(0, nodeCount - 1);

                for (int seg = 0; seg < splineSegments; seg++)
                {
                    float shade = 0.8f + (globalSegIndex % 2 == 0 ? 0.2f : 0f);
                    Gizmos.color = mainColor * shade;
                    Gizmos.color = new Color(Gizmos.color.r, Gizmos.color.g, Gizmos.color.b, 0.6f);

                    float segStart = (float)seg / splineSegments;
                    float segEnd = (float)(seg + 1) / splineSegments;

                    for (int s = 0; s <= SAMPLES_PER_SEGMENT; s++)
                    {
                        float t = Mathf.Lerp(segStart, segEnd, s / (float)SAMPLES_PER_SEGMENT);
                        SplineSample sample = spline.Evaluate(t);
                        
                        float localScale = GetScale(sample.position, MAX_GIZMO_SCALE);
                        Gizmos.DrawSphere(sample.position, PATH_DOT_RADIUS * localScale);
                    }

                    globalSegIndex++;
                }

                // Node labels & separators
                for (int n = 0; n < nodeCount; n++)
                {
                    float t = nodeCount > 1 ? (float)n / (nodeCount - 1) : 0f;
                    SplineSample nodeSample = spline.Evaluate(t);
                    Vector3 nodePos = nodeSample.position;

                    float localScale = GetScale(nodePos, MAX_GIZMO_SCALE); 

                    Gizmos.color = SeparatorColor;
                    Gizmos.DrawWireSphere(nodePos, NODE_SPHERE_RADIUS * localScale);

                    string railPrefix = rails.Count > 1 ? $"R{r}:" : "";
                    GUIStyle labelStyle = new GUIStyle(GUI.skin.label)
                    {
                        fontSize = 13,
                        fontStyle = FontStyle.Bold,
                        alignment = TextAnchor.MiddleCenter,
                        normal = { textColor = NodeLabelColor }
                    };

                    Vector3 labelPos = nodePos + Vector3.up * (1.1f * localScale);
                    Handles.Label(labelPos, $"{railPrefix}N{n}", labelStyle);
                }
            }
        }

        // --- Progress & Camera ---
        private static void DrawProgressPoint(CameraRig rig, float s)
        {
            float progress = rig.Rails.GlobalProgress;
            CameraPose pose = rig.Rails.CalculateTargetPose();

            Gizmos.color = ProgressSphereColor;
            Gizmos.DrawSphere(pose.position, SPHERE_RADIUS * s);

            Gizmos.color = DirectionColor;
            Vector3 forward = pose.rotation * Vector3.forward;
            Vector3 end = pose.position + forward * (DIRECTION_ARROW_LENGTH * s);
            
            Gizmos.DrawLine(pose.position, end);
            Gizmos.DrawSphere(end, (SPHERE_RADIUS * 0.5f) * s);
        }

        private static void DrawCameraFrustum(CameraRig rig, float s)
        {
            Camera cam = rig.TargetCamera;
            if (cam == null)
                return;
            
            Gizmos.color = FrustumColor;
            Matrix4x4 oldMatrix = Gizmos.matrix;
            
            Gizmos.matrix = Matrix4x4.TRS(cam.transform.position, cam.transform.rotation, Vector3.one);
            
            Gizmos.DrawFrustum(
                Vector3.zero,
                cam.fieldOfView,
                5f * s,
                cam.nearClipPlane,
                cam.aspect
            );
            
            Gizmos.matrix = oldMatrix;
        }

        private static void DrawLookAtLine(CameraRig rig, float s)
        {
            Transform lookAt = rig.CurrentLookAtTarget;
            Camera cam = rig.TargetCamera;
    
            if (lookAt == null || cam == null)
                return;
            
            Gizmos.color = LookAtLineColor;
            
            float targetScale = GetScale(lookAt.position, MAX_CAMERA_GIZMO_SCALE);
    
            Gizmos.DrawLine(cam.transform.position, lookAt.position);
            Gizmos.DrawWireSphere(lookAt.position, 0.3f * targetScale);
        }

        private static void DrawProgressLabel(CameraRig rig, float s)
        {
            if (rig.TargetCamera == null)
                return;

            Vector3 labelPos = rig.TargetCamera.transform.position + Vector3.up * (1.8f * s);
            
            GUIStyle style = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = ProgressSphereColor }
            };

            Handles.Label(labelPos, $"Rig Progress: {rig.Rails.GlobalProgress:P1}", style);
        }
    }
}