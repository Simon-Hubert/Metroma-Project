using UnityEngine;
using UnityEditor;
using Metroma.CameraTool.Timeline;
using Metroma.CameraTool.Modifiers;

namespace Metroma.CameraTool.Editor
{
    [CustomEditor(typeof(CameraHandheldMarker))]
    public class CameraHandheldMarkerInspector : UnityEditor.Editor
    {
        private SerializedProperty _profile;
        private SerializedProperty _deactivate;

        private static readonly Color CyanAccent = new Color(0.1f, 0.8f, 1f);

        private void OnEnable()
        {
            _profile = serializedObject.FindProperty("profile");
            _deactivate = serializedObject.FindProperty("deactivate");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space(4);
            DrawMarkerHeader();

            EditorGUILayout.PropertyField(_deactivate, new GUIContent("DEACTIVATE HANDHELD", "If checked, this marker will stop any active handheld motion."));

            if (!_deactivate.boolValue)
            {
                EditorGUILayout.Space(8);
                EditorGUILayout.PropertyField(_profile, new GUIContent("HANDHELD PROFILE"));

                if (_profile.objectReferenceValue != null)
                {
                    EditorGUILayout.Space(4);
                    if (GUILayout.Button("Select Profile Asset", EditorStyles.miniButton))
                    {
                        Selection.activeObject = _profile.objectReferenceValue;
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("Assign a Handheld Profile to activate organic camera motion.", MessageType.Warning);
                }
            }
            else
            {
                EditorGUILayout.Space(8);
                EditorGUILayout.HelpBox("This marker will STOP all handheld motion when reached.", MessageType.Info);
            }

            EditorGUILayout.Space(16);
            DrawTestButton();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawTestButton()
        {
            GUI.enabled = Application.isPlaying;
            Color oldCol = GUI.backgroundColor;
            GUI.backgroundColor = CyanAccent;

            string btnLabel = Application.isPlaying ? 
                (_deactivate.boolValue ? "🎥 STOP HANDHELD" : "🎥 TEST HANDHELD PROFILE") : 
                "⏸  PLAY MODE REQUIRED TO TEST";

            if (GUILayout.Button(btnLabel, GUILayout.Height(32)))
            {
                if (Camera.main)
                {
                    if (_deactivate.boolValue)
                    {
                        CameraModifiers.SetHandheld(Camera.main, false, null);
                    }
                    else if (_profile.objectReferenceValue != null)
                    {
                        CameraModifiers.SetHandheld(Camera.main, true,
                            (CameraHandheldProfile)_profile.objectReferenceValue);
                    }
                }
            }

            GUI.backgroundColor = oldCol;
            GUI.enabled = true;
        }

        private void DrawMarkerHeader()
        {
            var rect = EditorGUILayout.GetControlRect(false, 22);
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, 1), new Color(1, 1, 1, 0.1f));
            EditorGUI.DrawRect(new Rect(rect.x, rect.y + 20, rect.width, 2), CyanAccent);
            GUI.Label(new Rect(rect.x + 4, rect.y + 2, rect.width, 18), "CAMERA HANDHELD MARKER", EditorStyles.boldLabel);
            EditorGUILayout.Space(8);
        }
    }
}
