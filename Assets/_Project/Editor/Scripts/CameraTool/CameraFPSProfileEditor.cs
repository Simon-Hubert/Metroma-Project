using UnityEngine;
using UnityEditor;
using Metroma.CameraTool.Modifiers;

namespace Metroma.CameraTool.Editor
{
    /// <summary>
    /// Custom Inspector for CameraFPSProfile.
    /// Focuses on readability, visual polish, and ease of tuning for designers.
    /// </summary>
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
        private SerializedProperty _handheldProfile;
        private SerializedProperty _swayAmount;
        private SerializedProperty _swaySpeed;
        private SerializedProperty _tiltAmount;
        private SerializedProperty _tiltReturnSpeed;
        private SerializedProperty _rotationMomentum;
        private SerializedProperty _jitterAmount;
        private SerializedProperty _jitterSpeed;

        private static readonly Color CyanAccent = new Color(0.1f, 0.8f, 1f);
        private static readonly Color BackgroundDark = new Color(0.12f, 0.12f, 0.12f);
        private static readonly Color HeaderBg = new Color(0.18f, 0.18f, 0.18f);

        private void OnEnable()
        {
            _lookAction = serializedObject.FindProperty("lookAction");
            _sensitivity = serializedObject.FindProperty("sensitivity");
            _invertY = serializedObject.FindProperty("invertY");
            _pitchLimits = serializedObject.FindProperty("pitchLimits");
            _yawLimits = serializedObject.FindProperty("yawLimits");
            _inputSmoothing = serializedObject.FindProperty("inputSmoothing");
            _fovOverride = serializedObject.FindProperty("fovOverride");
            _handheldProfile = serializedObject.FindProperty("handheldProfile");
            _swayAmount = serializedObject.FindProperty("swayAmount");
            _swaySpeed = serializedObject.FindProperty("swaySpeed");
            _tiltAmount = serializedObject.FindProperty("tiltAmount");
            _tiltReturnSpeed = serializedObject.FindProperty("tiltReturnSpeed");
            _rotationMomentum = serializedObject.FindProperty("rotationMomentum");
            _jitterAmount = serializedObject.FindProperty("jitterAmount");
            _jitterSpeed = serializedObject.FindProperty("jitterSpeed");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawHeader();

            EditorGUILayout.Space(8);

            // --- 🎮 INPUT ---
            using (new SectionScope("🎮 INPUT & CONTROL", CyanAccent))
            {
                EditorGUILayout.PropertyField(_lookAction);
                if (_lookAction.objectReferenceValue == null)
                    EditorGUILayout.HelpBox("No Action assigned. Using Mouse fallback.", MessageType.None);

                DrawSeparator();
                
                EditorGUILayout.PropertyField(_sensitivity);
                EditorGUILayout.PropertyField(_invertY);
                EditorGUILayout.PropertyField(_inputSmoothing, new GUIContent("SMOOTHING (LAG)"));
                EditorGUILayout.PropertyField(_rotationMomentum, new GUIContent("MOMENTUM (WEIGHT)"));
            }

            // --- 🔒 CONSTRAINTS ---
            using (new SectionScope("🔒 ROTATION LIMITS", new Color(1f, 0.4f, 0.4f)))
            {
                DrawMinMaxProperty(_pitchLimits, "PITCH (VERTICAL)", -90, 90);
                DrawMinMaxProperty(_yawLimits, "YAW (HORIZONTAL)", -180, 180);
                
                var profile = (CameraFPSProfile)target;
                GUI.color = profile.IsYawUnlimited ? Color.gray : Color.white;
                EditorGUILayout.LabelField(profile.IsYawUnlimited ? "Yaw is Unlimited" : "Yaw is Constrained", EditorStyles.miniLabel);
                GUI.color = Color.white;
            }

            // --- ✨ JUICE & BREATH ---
            using (new SectionScope("✨ CINEMATIC JUICE", new Color(0.4f, 1f, 0.4f)))
            {
                EditorGUILayout.PropertyField(_handheldProfile, new GUIContent("HANDHELD PROFILE (ADVANCED)"));
                if (_handheldProfile.objectReferenceValue != null)
                {
                    EditorGUILayout.HelpBox("Using dedicated Handheld Profile. Manual settings below are disabled.", MessageType.None);
                }

                DrawSeparator();

                EditorGUILayout.PropertyField(_swayAmount, new GUIContent("SWAY (BREATH)"));
                EditorGUILayout.PropertyField(_swaySpeed, new GUIContent("SWAY SPEED"));
                
                DrawSeparator();

                EditorGUILayout.PropertyField(_tiltAmount, new GUIContent("DYNAMIC ROLL (TILT)"));
                EditorGUILayout.PropertyField(_tiltReturnSpeed);
            }

            // --- 📳 JITTER ---
            using (new SectionScope("📳 HANDHELD JITTER", new Color(1f, 0.8f, 0.4f)))
            {
                GUI.enabled = _handheldProfile.objectReferenceValue == null;
                EditorGUILayout.PropertyField(_jitterAmount, new GUIContent("JITTER (SHAKY)"));
                EditorGUILayout.PropertyField(_jitterSpeed, new GUIContent("JITTER SPEED"));
                GUI.enabled = true;
            }

            // --- 📷 LENS ---
            using (new SectionScope("📷 OPTICS", new Color(0.8f, 0.4f, 1f)))
            {
                EditorGUILayout.PropertyField(_fovOverride, new GUIContent("BASE FOV OVERRIDE"));
                if (_fovOverride.floatValue <= 0)
                    EditorGUILayout.LabelField("Keeping original camera FOV", EditorStyles.miniLabel);
            }

            EditorGUILayout.Space(10);
            serializedObject.ApplyModifiedProperties();
        }

        #region --- UI Helpers ---

        private void DrawHeader()
        {
            Rect rect = EditorGUILayout.GetControlRect(false, 34);
            EditorGUI.DrawRect(rect, HeaderBg);
            
            // Bottom accent line
            Rect accentRect = new Rect(rect.x, rect.yMax - 2, rect.width, 2);
            EditorGUI.DrawRect(accentRect, CyanAccent);

            GUIStyle labelStyle = new GUIStyle(EditorStyles.boldLabel);
            labelStyle.fontSize = 13;
            labelStyle.alignment = TextAnchor.MiddleLeft;
            labelStyle.normal.textColor = Color.white;

            Rect labelRect = new Rect(rect.x + 10, rect.y, rect.width - 20, rect.height);
            EditorGUI.LabelField(labelRect, "METROMA // FPS PROFILE", labelStyle);
        }

        private void DrawSeparator()
        {
            EditorGUILayout.Space(4);
            Rect rect = EditorGUILayout.GetControlRect(false, 1);
            rect.x += 10;
            rect.width -= 20;
            EditorGUI.DrawRect(rect, new Color(1, 1, 1, 0.05f));
            EditorGUILayout.Space(4);
        }

        private void DrawMinMaxProperty(SerializedProperty property, string label, float min, float max)
        {
            Vector2 val = property.vector2Value;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label, GUILayout.Width(EditorGUIUtility.labelWidth));
            val.x = EditorGUILayout.FloatField(val.x, GUILayout.Width(45));
            EditorGUILayout.MinMaxSlider(ref val.x, ref val.y, min, max);
            val.y = EditorGUILayout.FloatField(val.y, GUILayout.Width(45));
            EditorGUILayout.EndHorizontal();
            property.vector2Value = val;
        }

        private class SectionScope : System.IDisposable
        {
            public SectionScope(string title, Color accent)
            {
                EditorGUILayout.Space(4);
                Rect headerRect = EditorGUILayout.GetControlRect(false, 20);
                EditorGUI.DrawRect(headerRect, new Color(accent.r, accent.g, accent.b, 0.15f));
                
                // Vertical accent line
                Rect accentLine = new Rect(headerRect.x, headerRect.y, 3, headerRect.height);
                EditorGUI.DrawRect(accentLine, accent);

                GUIStyle headerStyle = new GUIStyle(EditorStyles.miniBoldLabel);
                headerStyle.normal.textColor = accent;
                EditorGUI.LabelField(new Rect(headerRect.x + 8, headerRect.y, headerRect.width, headerRect.height), title, headerStyle);

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUI.indentLevel++;
                EditorGUILayout.Space(2);
            }

            public void Dispose()
            {
                EditorGUILayout.Space(4);
                EditorGUI.indentLevel--;
                EditorGUILayout.EndVertical();
            }
        }

        #endregion
    }
}
