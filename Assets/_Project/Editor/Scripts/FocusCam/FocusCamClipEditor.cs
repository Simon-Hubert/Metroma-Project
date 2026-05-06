using UnityEditor;
using UnityEngine;
using Metroma.FocusCam;

namespace Metroma.FocusCam.Editor
{
    [CustomEditor(typeof(FocusCamClip))]
    public class FocusCamClipEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // --- Section: TARGETING (Where to look) ---
            EditorGUILayout.LabelField("🎯 TARGETING (Look At)", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            SerializedProperty modeProp = serializedObject.FindProperty("mode");
            EditorGUILayout.PropertyField(modeProp);
            
            FocusMode currentMode = (FocusMode)modeProp.enumValueIndex;
            
            if (currentMode == FocusMode.LookAtPoint)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("position"), new GUIContent("Target Position"));
            }
            else
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("rotation"), new GUIContent("Target Rotation"));
            }

            EditorGUILayout.Space(5);
            if (GUILayout.Button("📷 Capture Focus Target", GUILayout.Height(25)))
            {
                var sceneView = SceneView.lastActiveSceneView;
                if (sceneView != null && sceneView.camera != null)
                {
                    if (currentMode == FocusMode.LookAtPoint)
                        serializedObject.FindProperty("position").vector3Value = sceneView.camera.transform.position;
                    else
                        serializedObject.FindProperty("rotation").vector3Value = sceneView.camera.transform.rotation.eulerAngles;
                }
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(10);

            // --- Section: CAMERA PLACEMENT (Where to be) ---
            EditorGUILayout.LabelField("📍 CAMERA PLACEMENT (Position)", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            SerializedProperty overridePosProp = serializedObject.FindProperty("overridePosition");
            EditorGUILayout.PropertyField(overridePosProp, new GUIContent("Override Camera Position"));
            
            if (overridePosProp.boolValue)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("cameraPosition"), new GUIContent("Camera Position"));
                
                EditorGUILayout.Space(5);
                if (GUILayout.Button("📍 Capture Camera Position", GUILayout.Height(25)))
                {
                    var sceneView = SceneView.lastActiveSceneView;
                    if (sceneView != null && sceneView.camera != null)
                    {
                        serializedObject.FindProperty("cameraPosition").vector3Value = sceneView.camera.transform.position;
                    }
                }
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(10);

            // --- Section: OPTICS & EFFECTS ---
            EditorGUILayout.LabelField("🎥 OPTICS & EFFECTS", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            SerializedProperty overrideFOVProp = serializedObject.FindProperty("overrideFOV");
            EditorGUILayout.PropertyField(overrideFOVProp, new GUIContent("Override FOV (Zoom)"));
            
            if (overrideFOVProp.boolValue)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("fov"), new GUIContent("Field of View"));
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("roll"), new GUIContent("Dutch Angle (Roll)"));
            
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(10);

            // --- Section: EDITOR VISUALS ---
            EditorGUILayout.LabelField("🎨 EDITOR VISUALS", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            SerializedProperty useCustomColorProp = serializedObject.FindProperty("useCustomColor");
            EditorGUILayout.PropertyField(useCustomColorProp, new GUIContent("Use Custom Color"));
            
            if (useCustomColorProp.boolValue)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("customColor"), new GUIContent("Clip Color"));
            }
            
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(10);
            
            GUI.backgroundColor = new Color(0.2f, 0.6f, 1.0f);
            if (GUILayout.Button("🎬 Snap Camera", GUILayout.Height(30)))
            {
                Debug.Log("[FocusCamClipEditor] Teleport button clicked.");
                FocusCamClip clip = (FocusCamClip)target;
                if (clip != null)
                {
                    Vector3 finalPos = clip.cameraPosition;
                    UnityEngine.Camera mainCam = UnityEngine.Camera.main;
                    if (mainCam == null) mainCam = UnityEngine.Object.FindAnyObjectByType<UnityEngine.Camera>();

                    var sceneView = SceneView.lastActiveSceneView;
                    if (sceneView == null && SceneView.sceneViews.Count > 0)
                    {
                        sceneView = (SceneView)SceneView.sceneViews[0];
                    }
                    if (!clip.overridePosition)
                    {
                        if (mainCam != null) finalPos = mainCam.transform.position;
                        else if (sceneView != null && sceneView.camera != null) finalPos = sceneView.camera.transform.position;
                    }

                    Quaternion finalRot = Quaternion.identity;
                    
                    if (clip.mode == FocusMode.LookAtPoint)
                    {
                        Vector3 direction = (clip.position - finalPos).normalized;
                        if (direction != Vector3.zero) finalRot = Quaternion.LookRotation(direction, Vector3.up);
                        Debug.Log($"[FocusCamClipEditor] Mode: LookAtPoint. Target: {clip.position}");
                    }
                    else
                    {
                        finalRot = Quaternion.Euler(clip.rotation);
                        Debug.Log($"[FocusCamClipEditor] Mode: KeepOrientation. Rotation: {clip.rotation}");
                    }
                    finalRot *= Quaternion.Euler(0, 0, clip.roll);

                    if (mainCam != null)
                    {
                        mainCam.transform.SetPositionAndRotation(finalPos, finalRot);
                        if (clip.overrideFOV) mainCam.fieldOfView = clip.fov;
                        Debug.Log($"[FocusCamClipEditor] Successfully moved Game Camera: {mainCam.name}");
                    }
                    else
                    {
                        Debug.LogWarning("[FocusCamClipEditor] No active Game Camera found in the scene.");
                    }

                    if (sceneView != null)
                    {
                        sceneView.pivot = finalPos;
                        sceneView.rotation = finalRot;
                        sceneView.size = 0f;
                        sceneView.Repaint();
                        Debug.Log($"[FocusCamClipEditor] Set SceneView Pivot: {finalPos}, Rotation: {finalRot}, Size: 0");
                    }
                    else
                    {
                        Debug.LogWarning("[FocusCamClipEditor] No active Scene View available.");
                    }

                    UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
                }
            }
            GUI.backgroundColor = Color.white;

            serializedObject.ApplyModifiedProperties();
        }

        private void OnSceneGUI()
        {
            FocusCamClip clip = (FocusCamClip)target;
            if (clip == null) return;

            // 1. Draw Target Focus Point (Rose)
            if (clip.mode == FocusMode.LookAtPoint)
            {
                Color pinkColor = new Color(0.9f, 0.2f, 0.4f);
                Handles.color = pinkColor;
                Handles.SphereHandleCap(0, clip.position, Quaternion.identity, 0.5f, EventType.Repaint);
                Handles.Label(clip.position + Vector3.up * 0.7f, "Focus Point", EditorStyles.boldLabel);

                EditorGUI.BeginChangeCheck();
                Vector3 newTargetPos = Handles.PositionHandle(clip.position, Quaternion.identity);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(clip, "Move Focus Point");
                    clip.position = newTargetPos;
                    EditorUtility.SetDirty(clip);
                }
            }

            // 2. Draw Camera Placement (Blue)
            if (clip.overridePosition)
            {
                Color blueColor = new Color(0.2f, 0.5f, 0.9f);
                Handles.color = blueColor;
                
                // Draw a camera-like icon
                Handles.CubeHandleCap(0, clip.cameraPosition, Quaternion.identity, 0.4f, EventType.Repaint);
                Handles.Label(clip.cameraPosition + Vector3.up * 0.7f, "Cam Placement", EditorStyles.boldLabel);

                EditorGUI.BeginChangeCheck();
                Vector3 newCamPos = Handles.PositionHandle(clip.cameraPosition, Quaternion.identity);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(clip, "Move Camera Position");
                    clip.cameraPosition = newCamPos;
                    EditorUtility.SetDirty(clip);
                }

                // Draw line between camera and target
                Handles.color = new Color(blueColor.r, blueColor.g, blueColor.b, 0.3f);
                Handles.DrawDottedLine(clip.cameraPosition, clip.mode == FocusMode.LookAtPoint ? clip.position : clip.cameraPosition + Quaternion.Euler(clip.rotation) * Vector3.forward * 5f, 4f);
            }
            else if (Camera.main != null && clip.mode == FocusMode.LookAtPoint)
            {
                // Only draw look-at line if no position override
                Handles.color = new Color(0.9f, 0.2f, 0.4f, 0.3f);
                Handles.DrawDottedLine(Camera.main.transform.position, clip.position, 4f);
            }
        }
    }

}
