using UnityEditor;
using UnityEngine;
using Metroma.CameraTool;
using Metroma.CameraTool.Modules;

namespace Metroma.CameraTool.Editor
{
    [CustomEditor(typeof(CameraRig))]
    public class CameraRigEditor : UnityEditor.Editor
    {
        private CameraRig _rig;
        
        // Styles
        private GUIStyle _headerStyle;
        private GUIStyle _subHeaderStyle;

        // Colors
        private readonly Color _metromaPink = new Color(0.95f, 0.2f, 0.45f);
        private readonly Color _metromaBlue = new Color(0.15f, 0.6f, 1.0f);
        private readonly Color _metromaGreen = new Color(0.2f, 0.8f, 0.4f);
        private readonly Color _metromaDark = new Color(0.12f, 0.12f, 0.15f);

        private void OnEnable()
        {
            _rig = (CameraRig)target;
            
            if (!Application.isPlaying && _rig != null && _rig.Sequences != null)
            {
                var director = _rig.Sequences.Director;
                if (director != null)
                {
                    var window = UnityEditor.Timeline.TimelineEditor.GetWindow();
                    if (window != null) window.SetTimeline(director);
                }
            }
        }

        public override void OnInspectorGUI()
        {
            InitializeStyles();
            serializedObject.Update();

            DrawCustomHeader();
            EditorGUILayout.Space(15);

            // --- Debug ---
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                DrawSubHeader("🐞 Debug Overlay", Color.gray);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("showSceneGizmos"), new GUIContent("Show Scene Gizmos"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("showRuntimeHUD"), new GUIContent("Show HUD (Scene/Game)"));
            }

            EditorGUILayout.Space(10);

            // --- Hardware ---
            using (new EditorGUILayout.VerticalScope(GetSectionStyle(_metromaBlue)))
            {
                DrawSubHeader("🔌 Hardware Configuration", _metromaBlue);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("targetCamera"), new GUIContent("Main Camera"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("autoHandleTransform"), new GUIContent("Transform Control"));
            }

            EditorGUILayout.Space(10);

            // --- FocusCam Center ---
            using (new EditorGUILayout.VerticalScope(GetSectionStyle(_metromaPink)))
            {
                DrawSubHeader("🎯 FocusCam Management", _metromaPink);
                SequenceModule seq = _rig.Sequences;
                if (seq != null)
                {
                    SerializedObject seqSo = new SerializedObject(seq);
                    seqSo.Update();
                    EditorGUILayout.PropertyField(seqSo.FindProperty("playableDirector"), new GUIContent("Active Director"));
                    
                    EditorGUILayout.Space(8);
                    Rect lineRect = EditorGUILayout.GetControlRect(false, 1);
                    EditorGUI.DrawRect(lineRect, new Color(1,1,1, 0.1f));
                    EditorGUILayout.Space(8);

                    EditorGUILayout.LabelField("🛠️ QUICK TEST PLAYBACK", EditorStyles.miniBoldLabel);
                    using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                    {
                        EditorGUILayout.Space(2);
                        EditorGUILayout.LabelField("Target Focus Timeline", EditorStyles.miniLabel);
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            EditorGUI.BeginChangeCheck();
                            EditorGUILayout.PropertyField(seqSo.FindProperty("debugFocusTimeline"), GUIContent.none, GUILayout.MinWidth(100));
                            if (EditorGUI.EndChangeCheck())
                            {
                                seqSo.ApplyModifiedProperties();
                                if (seq.Director != null && seq.DebugFocusTimeline != null)
                                {
                                    seq.Director.playableAsset = seq.DebugFocusTimeline;
                                    var window = UnityEditor.Timeline.TimelineEditor.GetWindow();
                                    if (window != null) window.SetTimeline(seq.Director);
                                }
                            }
                            
                            GUI.backgroundColor = Application.isPlaying ? _metromaGreen : new Color(1f, 0.7f, 0.2f);
                            string btnLabel = Application.isPlaying ? "▶ Play Now" : "⚡ Enter Play";
                            if (GUILayout.Button(btnLabel, GUILayout.Width(110), GUILayout.Height(19)))
                            {
                                if (!Application.isPlaying)
                                {
                                    EditorPrefs.SetBool("Metroma.AutoPlayFocus", true);
                                    EditorApplication.isPlaying = true;
                                }
                                else seq.EditorTestFocusTimeline();
                            }
                            GUI.backgroundColor = Color.white;
                        }
                    }
                    seqSo.ApplyModifiedProperties();
                }
            }

            if (Application.isPlaying && EditorPrefs.GetBool("Metroma.AutoPlayFocus", false))
            {
                EditorPrefs.SetBool("Metroma.AutoPlayFocus", false);
                _rig.Sequences.EditorTestFocusTimeline();
            }

            EditorGUILayout.Space(10);

            // --- Module Status ---
            using (new EditorGUILayout.VerticalScope(GetSectionStyle(_metromaGreen)))
            {
                DrawSubHeader("🧩 Module Orchestration", _metromaGreen);
                DrawModuleStatus("Sequence", _rig.Sequences != null, _rig.Sequences != null && _rig.Sequences.IsPlayingFocus ? "PLAYING FOCUS" : "IDLE");
                DrawModuleStatus("Transition", _rig.Transitions != null, _rig.Transitions != null && _rig.Transitions.IsActive ? "TRANSITIONING" : "IDLE");
                DrawModuleStatus("FPS", _rig.FPS != null, _rig.FPS != null && _rig.FPS.IsActive ? "FPS ACTIVE" : "IDLE");
            }

            EditorGUILayout.Space(15);
            if (Application.isPlaying)
            {
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    DrawSubHeader("🐞 Runtime Inspector", Color.gray);
                    EditorGUILayout.LabelField("Current Position", _rig.CurrentPose.position.ToString(), EditorStyles.miniLabel);
                    if (GUILayout.Button("🔄 REGAIN RIG CONTROL", GUILayout.Height(30))) _rig.Transitions.ReturnToRigControl(5f);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void OnSceneGUI()
        {
            if (!_rig.showRuntimeHUD) return;

            Handles.BeginGUI();
            
            Rect rect = new Rect(10, 10, 200, 80);
            GUI.color = new Color(0, 0, 0, 0.7f);
            GUI.Box(rect, "");
            GUI.color = Color.white;

            using (new GUILayout.AreaScope(new Rect(rect.x + 10, rect.y + 5, rect.width - 20, rect.height - 10)))
            {
                GUIStyle titleStyle = new GUIStyle(EditorStyles.miniBoldLabel) { normal = { textColor = _metromaPink } };
                GUILayout.Label("METROMA CAMERA RIG", titleStyle);
                
                GUIStyle valueStyle = new GUIStyle(EditorStyles.miniLabel) { normal = { textColor = Color.white } };
                GUILayout.Label($"Pose: {_rig.CurrentPose.position.x:F1}, {_rig.CurrentPose.position.y:F1}, {_rig.CurrentPose.position.z:F1}", valueStyle);
                GUILayout.Label($"FOV: {_rig.CurrentPose.fov:F1}°", valueStyle);
                
                string mode = "IDLE";
                if (_rig.Sequences != null && _rig.Sequences.IsPlayingFocus) mode = "FOCUS PLAYING";
                else if (_rig.Transitions != null && _rig.Transitions.IsActive) mode = "TRANSITIONING";
                
                GUILayout.Label($"State: {mode}", valueStyle);
            }

            Handles.EndGUI();
        }

        private void InitializeStyles()
        {
            if (_headerStyle != null) return;
            _headerStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 18, alignment = TextAnchor.MiddleLeft, normal = { textColor = Color.white } };
            _subHeaderStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 12, margin = new RectOffset(0, 0, 0, 5), normal = { textColor = new Color(0.9f, 0.9f, 0.9f) } };
        }

        private GUIStyle GetSectionStyle(Color accent)
        {
            GUIStyle style = new GUIStyle(EditorStyles.helpBox);
            style.padding = new RectOffset(10, 10, 10, 10);
            return style;
        }

        private void DrawCustomHeader()
        {
            Rect rect = EditorGUILayout.GetControlRect(false, 50);
            EditorGUI.DrawRect(rect, _metromaDark);
            EditorGUI.DrawRect(new Rect(rect.x, rect.y + rect.height - 2, rect.width, 2), _metromaPink);
            Rect iconRect = new Rect(rect.x + 10, rect.y + 10, 30, 30);
            EditorGUI.DrawRect(iconRect, _metromaPink);
            GUI.Label(new Rect(rect.x + 50, rect.y, rect.width - 60, rect.height), "METROMA CAMERA RIG", _headerStyle);
        }

        private void DrawSubHeader(string title, Color color)
        {
            Rect rect = EditorGUILayout.GetControlRect(false, 20);
            EditorGUI.DrawRect(new Rect(rect.x - 2, rect.y, 3, rect.height), color);
            GUI.Label(new Rect(rect.x + 8, rect.y, rect.width, rect.height), title, _subHeaderStyle);
            EditorGUILayout.Space(5);
        }

        private void DrawModuleStatus(string name, bool exists, string status)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(name, GUILayout.Width(120));
                if (!exists)
                {
                    GUI.color = Color.red;
                    GUILayout.Label("MISSING", EditorStyles.miniBoldLabel);
                }
                else
                {
                    bool isIdle = status == "IDLE";
                    GUI.color = isIdle ? new Color(0.5f, 0.5f, 0.5f, 0.3f) : _metromaGreen;
                    Rect badgeRect = EditorGUILayout.GetControlRect(false, 16, GUILayout.Width(100));
                    EditorGUI.DrawRect(badgeRect, GUI.color);
                    GUIStyle labelStyle = new GUIStyle(EditorStyles.miniLabel) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, normal = { textColor = isIdle ? Color.white : Color.black } };
                    GUI.Label(badgeRect, status, labelStyle);
                }
                GUI.color = Color.white;
            }
        }
    }
}
