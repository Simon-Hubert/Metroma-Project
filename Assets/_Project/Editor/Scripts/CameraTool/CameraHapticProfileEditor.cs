using UnityEngine;
using UnityEditor;
using Metroma.CameraTool.Modifiers;

namespace Metroma.CameraTool.Editor
{
    [CustomEditor(typeof(CameraHapticProfile))]
    public class CameraHapticProfileEditor : UnityEditor.Editor
    {
        private SerializedProperty _intensityCurve;
        private SerializedProperty _lowFreq;
        private SerializedProperty _highFreq;
        private SerializedProperty _duration;
        private SerializedProperty _usePattern;
        private SerializedProperty _pulseCount;
        private SerializedProperty _pulseInterval;

        private static readonly Color CyanAccent = new Color(0.1f, 0.8f, 1f);

        private void OnEnable()
        {
            _intensityCurve = serializedObject.FindProperty("intensityCurve");
            _lowFreq = serializedObject.FindProperty("lowFreqMultiplier");
            _highFreq = serializedObject.FindProperty("highFreqMultiplier");
            _duration = serializedObject.FindProperty("duration");
            _usePattern = serializedObject.FindProperty("usePattern");
            _pulseCount = serializedObject.FindProperty("pulseCount");
            _pulseInterval = serializedObject.FindProperty("pulseInterval");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawProfileHeader();

            EditorGUILayout.Space(10);

            // 1. Timing Section
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("🕒 TIMING & PATTERN", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            
            EditorGUILayout.PropertyField(_usePattern, new GUIContent("USE PULSE PATTERN"));
            
            if (_usePattern.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_pulseCount);
                EditorGUILayout.PropertyField(_pulseInterval);
                
                float autoDur = _pulseInterval.floatValue * (_pulseCount.intValue + 1);
                GUI.enabled = false;
                EditorGUILayout.TextField("Duration (Auto)", $"{autoDur:F2}s");
                GUI.enabled = true;
                EditorGUI.indentLevel--;
            }
            else
            {
                EditorGUILayout.PropertyField(_duration);
            }
            
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(10);

            // 2. Response Section
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("🎮 INTENSITY & MOTORS", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(_intensityCurve);
            
            EditorGUILayout.Space(5);
            EditorGUILayout.PropertyField(_lowFreq, new GUIContent("LOW FREQ (RUMBLE)"));
            EditorGUILayout.PropertyField(_highFreq, new GUIContent("HIGH FREQ (JITTER)"));
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
            EditorGUI.LabelField(labelRect, "HAPTIC PROFILE SETTINGS", style);
        }
    }
}
