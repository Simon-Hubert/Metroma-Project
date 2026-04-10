using UnityEngine;
using UnityEditor;
using Metroma.CameraTool.Timeline;
using Metroma.CameraTool.Modifiers;

namespace Metroma.CameraTool.Editor
{
    [CustomEditor(typeof(CameraHapticMarker))]
    public class CameraHapticMarkerInspector : UnityEditor.Editor
    {
        private SerializedProperty _profile;
        private SerializedProperty _intensityCurve;
        private SerializedProperty _lowFreq;
        private SerializedProperty _highFreq;
        private SerializedProperty _duration;

        private static readonly Color CyanAccent = new Color(0.1f, 0.8f, 1f);
        private static readonly Color RumbleColor = new Color(1f, 0.4f, 0.1f);
        private static readonly Color JitterColor = new Color(0.2f, 0.9f, 0.5f);

        private void OnEnable()
        {
            _profile = serializedObject.FindProperty("profile");
            _intensityCurve = serializedObject.FindProperty("intensityCurve");
            _lowFreq = serializedObject.FindProperty("lowFreq");
            _highFreq = serializedObject.FindProperty("highFreq");
            _duration = serializedObject.FindProperty("duration");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space(4);
            DrawMarkerHeader();

            EditorGUILayout.PropertyField(_profile);

            if (_profile.objectReferenceValue != null)
            {
                EditorGUILayout.Space(8);
                EditorGUILayout.HelpBox("SETTINGS CONTROLLED BY PROFILE\nIndividual overrides are hidden.", MessageType.Info);
                
                if (GUILayout.Button("Select Profile Asset", EditorStyles.miniButton))
                {
                    Selection.activeObject = _profile.objectReferenceValue;
                }
            }
            else
            {
                EditorGUILayout.Space(12);
                DrawSubHeader("🎮  DIRECT HAPTIC SETTINGS");
                
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_duration);
                EditorGUILayout.PropertyField(_intensityCurve);
                
                EditorGUILayout.Space(8);
                DrawHapticSliders();
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space(16);
            DrawTestButton();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawMarkerHeader()
        {
            var rect = EditorGUILayout.GetControlRect(false, 22);
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, 1), new Color(1, 1, 1, 0.1f));
            EditorGUI.DrawRect(new Rect(rect.x, rect.y + 20, rect.width, 2), RumbleColor);
            GUI.Label(new Rect(rect.x + 4, rect.y + 2, rect.width, 18), "GAMEPAD HAPTIC MARKER", EditorStyles.boldLabel);
            EditorGUILayout.Space(8);
        }

        private void DrawSubHeader(string title)
        {
            EditorGUILayout.LabelField(title, EditorStyles.miniBoldLabel);
            var rect = GUILayoutUtility.GetLastRect();
            EditorGUI.DrawRect(new Rect(rect.x, rect.yMax, rect.width, 1), new Color(1, 1, 1, 0.05f));
            EditorGUILayout.Space(4);
        }

        private void DrawHapticSliders()
        {
            // Low Freq (Rumble)
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(_lowFreq, new GUIContent("Low Freq (Rumble)"));
            DrawPowerBar(_lowFreq.floatValue, RumbleColor);
            EditorGUILayout.EndHorizontal();

            // High Freq (Jitter)
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(_highFreq, new GUIContent("High Freq (Jitter)"));
            DrawPowerBar(_highFreq.floatValue, JitterColor);
            EditorGUILayout.EndHorizontal();
        }

        private void DrawPowerBar(float value, Color color)
        {
            var rect = GUILayoutUtility.GetRect(50, 14, GUILayout.Width(60));
            rect.y += 2;
            EditorGUI.DrawRect(rect, new Color(0, 0, 0, 0.3f));
            
            // Saturation feedback
            bool saturated = value > 1.0f;
            Color barColor = saturated ? Color.Lerp(color, Color.red, 0.5f) : color;
            
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width * Mathf.Min(1f, value), rect.height), barColor);
            
            if (saturated)
            {
                var labelStyle = new GUIStyle(EditorStyles.miniLabel) { alignment = TextAnchor.MiddleCenter, normal = { textColor = Color.white }, fontStyle = FontStyle.Bold };
                GUI.Label(rect, "MAX+", labelStyle);
            }
        }

        private void DrawTestButton()
        {
            GUI.enabled = Application.isPlaying;
            Color oldCol = GUI.backgroundColor;
            GUI.backgroundColor = RumbleColor;

            if (GUILayout.Button(Application.isPlaying ? "🎮  TEST VIBRATION" : "⏸  PLAY MODE REQUIRED TO TEST", GUILayout.Height(32)))
            {
                CameraHapticMarker marker = (CameraHapticMarker)target;
                if (Camera.main != null)
                {
                    var profile = (CameraHapticProfile)_profile.objectReferenceValue;
                    if (profile != null)
                        CameraModifiers.DoHaptic(Camera.main, profile);
                    else
                        CameraModifiers.DoHaptic(Camera.main, _intensityCurve.animationCurveValue, _lowFreq.floatValue, _highFreq.floatValue, _duration.floatValue);
                }
            }

            GUI.backgroundColor = oldCol;
            GUI.enabled = true;
        }
    }
}
