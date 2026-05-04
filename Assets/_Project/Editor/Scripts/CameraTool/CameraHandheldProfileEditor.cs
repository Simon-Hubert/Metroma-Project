using UnityEngine;
using UnityEditor;
using Metroma.CameraTool.Modifiers;

namespace Metroma.CameraTool.Editor
{
    [CustomEditor(typeof(CameraHandheldProfile))]
    public class CameraHandheldProfileEditor : UnityEditor.Editor
    {
        private SerializedProperty _posIntensity;
        private SerializedProperty _rotIntensity;
        private SerializedProperty _roughness;
        private SerializedProperty _influence;
        private SerializedProperty _breathCurve;
        private SerializedProperty _breathSpeed;

        private static readonly Color CyanAccent = new Color(0.1f, 0.8f, 1f);

        private void OnEnable()
        {
            _posIntensity = serializedObject.FindProperty("positionIntensity");
            _rotIntensity = serializedObject.FindProperty("rotationIntensity");
            _roughness = serializedObject.FindProperty("roughness");
            _influence = serializedObject.FindProperty("influence");
            _breathCurve = serializedObject.FindProperty("breathCurve");
            _breathSpeed = serializedObject.FindProperty("breathSpeed");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawProfileHeader();

            EditorGUILayout.Space(10);

            // 1. Noise Section
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("🎲 NOISE SETTINGS", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(_posIntensity, new GUIContent("POS INTENSITY (XYZ)"));
            EditorGUILayout.PropertyField(_rotIntensity, new GUIContent("ROT INTENSITY (XYZ)"));
            EditorGUILayout.PropertyField(_roughness);
            EditorGUILayout.PropertyField(_influence);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("damping"), new GUIContent("DAMPING (INERTIA)"));
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(10);

            // 2. Breathing Section
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("🫁 BREATHING (RHYTHMIC)", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(_breathCurve);
            EditorGUILayout.PropertyField(_breathSpeed);
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
            EditorGUI.LabelField(labelRect, "HANDHELD PROFILE SETTINGS", style);
        }
    }
}
