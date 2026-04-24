using UnityEngine;
using UnityEditor;
using Metroma.CameraTool.Timeline;
using Metroma.CameraTool.Modifiers;

namespace Metroma.CameraTool.Editor
{
    [CustomEditor(typeof(CameraFPSMarker))]
    public class CameraFPSMarkerInspector : UnityEditor.Editor
    {
        private SerializedProperty _profile;
        private SerializedProperty _deactivate;
        private SerializedProperty _startPosition;
        private SerializedProperty _startRotation;
        private SerializedProperty _transitionDuration;
        private SerializedProperty _transitionCurve;
        private SerializedProperty _exitTransitionDuration;
        private SerializedProperty _exitTransitionCurve;
        private SerializedProperty _fpsDuration;

        private static readonly Color CyanAccent = new Color(0.1f, 0.8f, 1f);

        private void OnEnable()
        {
            _profile = serializedObject.FindProperty("profile");
            _deactivate = serializedObject.FindProperty("deactivate");
            _startPosition = serializedObject.FindProperty("startPosition");
            _startRotation = serializedObject.FindProperty("startRotation");
            _transitionDuration = serializedObject.FindProperty("transitionDuration");
            _transitionCurve = serializedObject.FindProperty("transitionCurve");
            _exitTransitionDuration = serializedObject.FindProperty("exitTransitionDuration");
            _exitTransitionCurve = serializedObject.FindProperty("exitTransitionCurve");
            _fpsDuration = serializedObject.FindProperty("fpsDuration");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space(4);
            DrawMarkerHeader();

            EditorGUILayout.PropertyField(_deactivate, new GUIContent("DEACTIVATE FPS MODE", "If checked, this marker will stop FPS observation when reached."));

            if (!_deactivate.boolValue)
            {
                EditorGUILayout.Space(8);
                EditorGUILayout.PropertyField(_profile, new GUIContent("FPS PROFILE"));

                if (_profile.objectReferenceValue != null)
                {
                    EditorGUILayout.Space(4);
                    if (GUILayout.Button("Select Profile Asset", EditorStyles.miniButton))
                    {
                        Selection.activeObject = _profile.objectReferenceValue;
                    }

                    EditorGUILayout.Space(12);
                    DrawSubHeader("📍  START POSE");

                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(_startPosition, new GUIContent("Position"));
                    EditorGUILayout.PropertyField(_startRotation, new GUIContent("Rotation (Euler)"));
                    EditorGUI.indentLevel--;

                    EditorGUILayout.Space(4);
                    if (GUILayout.Button("📷 Capture from Scene Camera", EditorStyles.miniButton))
                    {
                        var sceneView = SceneView.lastActiveSceneView;
                        if (sceneView != null && sceneView.camera != null)
                        {
                            _startPosition.vector3Value = sceneView.camera.transform.position;
                            _startRotation.vector3Value = sceneView.camera.transform.rotation.eulerAngles;
                        }
                    }

                    EditorGUILayout.Space(8);
                    DrawSubHeader("🔀  TRANSITIONS");

                    EditorGUI.indentLevel++;
                    EditorGUILayout.LabelField("ENTER", EditorStyles.miniBoldLabel);
                    EditorGUILayout.PropertyField(_transitionDuration, new GUIContent("Duration"));
                    EditorGUILayout.PropertyField(_transitionCurve, new GUIContent("Easing Curve"));
                    
                    EditorGUILayout.Space(4);
                    EditorGUILayout.LabelField("EXIT", EditorStyles.miniBoldLabel);
                    EditorGUILayout.PropertyField(_exitTransitionDuration, new GUIContent("Duration"));
                    EditorGUILayout.PropertyField(_exitTransitionCurve, new GUIContent("Easing Curve"));
                    EditorGUI.indentLevel--;

                    EditorGUILayout.Space(8);
                    DrawSubHeader("⏱  DURATION");

                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(_fpsDuration, new GUIContent("FPS Duration"));

                    if (_fpsDuration.floatValue <= 0f)
                        EditorGUILayout.HelpBox("INFINITE — Exit only via code: rig.FPS.DisableFPS()", MessageType.Info);
                    else
                        EditorGUILayout.HelpBox($"Auto-exit after {_fpsDuration.floatValue}s.", MessageType.None);

                    EditorGUI.indentLevel--;
                }
                else
                {
                    EditorGUILayout.HelpBox("Assign an FPS Profile to activate observation mode.", MessageType.Warning);
                }
            }
            else
            {
                EditorGUILayout.Space(8);
                EditorGUILayout.HelpBox("This marker will STOP FPS observation when reached.", MessageType.Info);
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
                (_deactivate.boolValue ? "👁 STOP FPS MODE" : "👁 TEST FPS OBSERVATION") :
                "⏸  PLAY MODE REQUIRED TO TEST";

            if (GUILayout.Button(btnLabel, GUILayout.Height(32)))
            {
                if (CameraRig.Active != null)
                {
                    if (_deactivate.boolValue)
                    {
                        CameraRig.Active.FPS.DisableFPS();
                    }
                    else if (_profile.objectReferenceValue != null)
                    {
                        var rig = CameraRig.Active;
                        var prof = (CameraFPSProfile)_profile.objectReferenceValue;
                        Vector3 pos = _startPosition.vector3Value;
                        Quaternion rot = Quaternion.Euler(_startRotation.vector3Value);
                        rig.FPS.EnableFPS(pos, rot, prof, _fpsDuration.floatValue, _exitTransitionDuration.floatValue, _exitTransitionCurve.animationCurveValue);
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
            GUI.Label(new Rect(rect.x + 4, rect.y + 2, rect.width, 18), "CAMERA FPS OBSERVATION MARKER", EditorStyles.boldLabel);
            EditorGUILayout.Space(8);
        }

        private void DrawSubHeader(string title)
        {
            EditorGUILayout.LabelField(title, EditorStyles.miniBoldLabel);
            var rect = GUILayoutUtility.GetLastRect();
            EditorGUI.DrawRect(new Rect(rect.x, rect.yMax, rect.width, 1), new Color(1, 1, 1, 0.05f));
            EditorGUILayout.Space(4);
        }
    }
}
