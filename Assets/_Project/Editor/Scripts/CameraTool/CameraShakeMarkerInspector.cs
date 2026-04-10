using UnityEngine;
using UnityEditor;
using Metroma.CameraTool.Timeline;
using Metroma.CameraTool.Modifiers;

namespace Metroma.CameraTool.Editor
{
    [CustomEditor(typeof(CameraShakeMarker))]
    public class CameraShakeMarkerInspector : UnityEditor.Editor
    {
        private SerializedProperty _profile;
        private SerializedProperty _intensity;
        private SerializedProperty _duration;
        private SerializedProperty _roughness;
        private SerializedProperty _fadeOut;
        private SerializedProperty _intensityCurve;
        private SerializedProperty _syncHaptics;

        private static readonly Color CyanAccent = new Color(0.1f, 0.8f, 1f);

        private void OnEnable()
        {
            _profile = serializedObject.FindProperty("profile");
            _intensity = serializedObject.FindProperty("intensity");
            _duration = serializedObject.FindProperty("duration");
            _roughness = serializedObject.FindProperty("roughness");
            _fadeOut = serializedObject.FindProperty("fadeOut");
            _intensityCurve = serializedObject.FindProperty("intensityCurve");
            _syncHaptics = serializedObject.FindProperty("syncHaptics");
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
                EditorGUILayout.HelpBox("SETTINGS CONTROLLED BY PROFILE\nIndividual overrides are hidden to avoid confusion.", MessageType.Info);
                
                if (GUILayout.Button("Select Profile Asset", EditorStyles.miniButton))
                {
                    Selection.activeObject = _profile.objectReferenceValue;
                }
            }
            else
            {
                EditorGUILayout.Space(12);
                DrawSubHeader("💥  SHAKE OVERRIDES");
                
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_intensity);
                EditorGUILayout.PropertyField(_duration);
                EditorGUILayout.PropertyField(_roughness);
                EditorGUILayout.PropertyField(_fadeOut);
                EditorGUILayout.PropertyField(_intensityCurve);
                EditorGUI.indentLevel--;

                EditorGUILayout.Space(8);
                DrawSubHeader("🎮  HAPTIC SYNC");
                
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_syncHaptics, new GUIContent("Enable Controller Vibration"));
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
            EditorGUI.DrawRect(new Rect(rect.x, rect.y + 20, rect.width, 2), CyanAccent);
            GUI.Label(new Rect(rect.x + 4, rect.y + 2, rect.width, 18), "CAMERA SHAKE MARKER", EditorStyles.boldLabel);
            EditorGUILayout.Space(8);
        }

        private void DrawSubHeader(string title)
        {
            EditorGUILayout.LabelField(title, EditorStyles.miniBoldLabel);
            var rect = GUILayoutUtility.GetLastRect();
            EditorGUI.DrawRect(new Rect(rect.x, rect.yMax, rect.width, 1), new Color(1, 1, 1, 0.05f));
            EditorGUILayout.Space(4);
        }

        private void DrawTestButton()
        {
            GUI.enabled = Application.isPlaying;
            Color oldCol = GUI.backgroundColor;
            GUI.backgroundColor = CyanAccent;

            if (GUILayout.Button(Application.isPlaying ? "▶  TEST SHAKE & VIBRATION" : "⏸  PLAY MODE REQUIRED TO TEST", GUILayout.Height(32)))
            {
                CameraShakeMarker marker = (CameraShakeMarker)target;
                if (Camera.main != null)
                {
                    // Call the modifiers directly to be safe and avoid tool dependency in editor
                    if (marker.Intensity > 0)
                    {
                        var profile = (CameraShakeProfile)_profile.objectReferenceValue;
                        if (profile != null)
                            CameraModifiers.DoShake(Camera.main, profile, marker.Duration);
                        else
                            CameraModifiers.DoShake(Camera.main, _intensity.floatValue, _duration.floatValue, _roughness.floatValue, _fadeOut.boolValue, _intensityCurve.animationCurveValue, _syncHaptics.boolValue);
                    }
                }
            }

            GUI.backgroundColor = oldCol;
            GUI.enabled = true;
        }
    }
}
