using UnityEngine;
using UnityEditor;
using Metroma.CameraTool.Modifiers;

namespace Metroma.CameraTool.Editor
{
    [CustomEditor(typeof(CameraFPSProfile))]
    public class CameraFPSProfileEditor : UnityEditor.Editor
    {
        private SerializedProperty _lookAction;
        private SerializedProperty _sensitivity;
        private SerializedProperty _invertY;
        private SerializedProperty _pitchLimits;
        private SerializedProperty _yawLimits;
        private SerializedProperty _inputSmoothing;
        private SerializedProperty _fovOverride;

        private static readonly Color CyanAccent = new Color(0.1f, 0.8f, 1f);

        private void OnEnable()
        {
            _lookAction = serializedObject.FindProperty("lookAction");
            _sensitivity = serializedObject.FindProperty("sensitivity");
            _invertY = serializedObject.FindProperty("invertY");
            _pitchLimits = serializedObject.FindProperty("pitchLimits");
            _yawLimits = serializedObject.FindProperty("yawLimits");
            _inputSmoothing = serializedObject.FindProperty("inputSmoothing");
            _fovOverride = serializedObject.FindProperty("fovOverride");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawProfileHeader();

            EditorGUILayout.Space(10);

            // 1. Input Section
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("🎮 INPUT SETTINGS", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(_lookAction, new GUIContent("LOOK ACTION"));

            if (_lookAction.objectReferenceValue == null)
                EditorGUILayout.HelpBox("No InputAction assigned. Will fallback to Mouse.current.delta.", MessageType.Info);

            EditorGUILayout.PropertyField(_sensitivity, new GUIContent("SENSITIVITY (X / Y)"));
            EditorGUILayout.PropertyField(_invertY, new GUIContent("INVERT Y AXIS"));
            EditorGUILayout.PropertyField(_inputSmoothing, new GUIContent("SMOOTHING"));
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(10);

            // 2. Rotation Constraints Section
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("🔒 ROTATION CONSTRAINTS", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(_pitchLimits, new GUIContent("PITCH LIMITS (MIN / MAX)"));
            EditorGUILayout.PropertyField(_yawLimits, new GUIContent("YAW LIMITS (MIN / MAX)"));
            
            var profile = (CameraFPSProfile)target;
            if (profile.IsYawUnlimited)
            {
                EditorGUILayout.HelpBox("Yaw is UNLIMITED (full 360° rotation).", MessageType.None);
            }
            else
            {
                EditorGUILayout.HelpBox($"Yaw is CONSTRAINED to [{profile.yawLimits.x}° , {profile.yawLimits.y}°].", MessageType.None);
            }

            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(10);

            // 3. Visual Section
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("📷 VISUAL OVERRIDES", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(_fovOverride, new GUIContent("FOV OVERRIDE"));

            if (_fovOverride.floatValue <= 0f)
            {
                EditorGUILayout.HelpBox("FOV will remain unchanged during FPS mode.", MessageType.None);
            }
            else
            {
                EditorGUILayout.HelpBox($"FOV will be set to {_fovOverride.floatValue}° during FPS mode.", MessageType.None);
            }

            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(10);

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawProfileHeader()
        {
            var rect = EditorGUILayout.GetControlRect(false, 30);
            EditorGUI.DrawRect(rect, new Color(0.15f, 0.15f, 0.15f));
            
            var lineRect = new Rect(rect.x, rect.yMax - 2, rect.width, 2);
            EditorGUI.DrawRect(lineRect, CyanAccent);

            var labelRect = new Rect(rect.x + 10, rect.y, rect.width - 20, rect.height);
            GUIStyle style = new GUIStyle(EditorStyles.whiteLargeLabel);
            style.alignment = TextAnchor.MiddleLeft;
            style.fontStyle = FontStyle.Bold;
            EditorGUI.LabelField(labelRect, "FPS OBSERVATION PROFILE", style);
        }
    }
}
