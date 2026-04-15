using UnityEngine;
using UnityEditor;
using Metroma.CameraTool;
using Metroma.CameraTool.Timeline;
using Dreamteck.Splines;
using UnityEngine.Timeline;
using UnityEngine.Playables;
using System.Collections.Generic;
using Metroma.CameraTool.Modifiers;
using UnityEditor.Timeline;

namespace Metroma.CameraTool.Editor
{
    [CustomEditor(typeof(CameraTool))]
    public class CameraToolEditor : UnityEditor.Editor
    {
        // ── Serialized Properties ────────────────────────────────────
        private SerializedProperty _splineRails;
        private SerializedProperty _targetCamera;
        private SerializedProperty _playableDirector;
        private SerializedProperty _lookAtTarget;
        private SerializedProperty _lookAtTargets;
        private SerializedProperty _splineProgress;
        private SerializedProperty _lookAtWeight;
        private SerializedProperty _chapters;
        private SerializedProperty _onChapterStart;
        private SerializedProperty _onChapterEnd;

        // --- Persistence Properties ---
        private SerializedProperty _selectedChapterIndex;
        private SerializedProperty _foldReferences;
        private SerializedProperty _foldChapters;
        private SerializedProperty _foldSegments;
        private SerializedProperty _foldAnimation;
        private SerializedProperty _foldEvents;
        private SerializedProperty _foldViewport;
        private SerializedProperty _foldHaptics;
        private SerializedProperty _foldDebug;
        // ── Editor State ─────────────────────────────────────────────
        private SerializedProperty _autoHandleCamera;
        private bool _isCameraLocked;

        private bool _showHud;
        private bool _showGrid = true;
        private bool _showLetterbox = true;
        private float _letterboxHeight = 0.12f;

        // ── Preview State ────────────────────────────────────────────
        private bool _isPreviewing;
        private double _previewStartTime;
        private float _previewTotalDuration;
        private float[] _previewSegStarts;
        private float[] _previewSegDurations;
        private AnimationCurve[] _previewSegCurves;

        // ── Style Cache ──────────────────────────────────────────────
        private static GUIStyle _headerStyle;
        private static GUIStyle _statusOkStyle;
        private static GUIStyle _statusBadStyle;
        private static GUIStyle _sectionTitleStyle;

        // ── Colors ───────────────────────────────────────────────────
        private static readonly Color CyanAccent = new Color(0.1f, 0.8f, 1f);
        private static readonly Color HeaderBg = new Color(0.08f, 0.08f, 0.08f, 1f);
        private static readonly Color OkColor = new Color(0.2f, 0.85f, 0.4f);
        private static readonly Color BadColor = new Color(1f, 0.3f, 0.4f);
        private static readonly Color WaitBarColor = new Color(1f, 0.6f, 0.1f, 0.4f);
        private static readonly Color LineColor = new Color(1f, 1f, 1f, 0.08f);

        private void OnEnable()
        {
            _splineRails = serializedObject.FindProperty("splineRails");
            _targetCamera = serializedObject.FindProperty("targetCamera");
            _playableDirector = serializedObject.FindProperty("playableDirector");
            _lookAtTarget = serializedObject.FindProperty("lookAtTarget");
            _lookAtTargets = serializedObject.FindProperty("lookAtTargets");
            _autoHandleCamera = serializedObject.FindProperty("autoHandleCamera");
            _splineProgress = serializedObject.FindProperty("splineProgress");
            _lookAtWeight = serializedObject.FindProperty("lookAtWeight");
            _chapters = serializedObject.FindProperty("chapters");
            _onChapterStart = serializedObject.FindProperty("onChapterStart");
            _onChapterEnd = serializedObject.FindProperty("onChapterEnd");

            // --- UI Persistence ---
            _selectedChapterIndex = serializedObject.FindProperty("selectedChapterIndex");
            _foldReferences = serializedObject.FindProperty("foldReferences");
            _foldChapters = serializedObject.FindProperty("foldChapters");
            _foldSegments = serializedObject.FindProperty("foldSegments");
            _foldAnimation = serializedObject.FindProperty("foldAnimation");
            _foldEvents = serializedObject.FindProperty("foldEvents");
            _foldViewport = serializedObject.FindProperty("foldViewport");
            _foldHaptics = serializedObject.FindProperty("foldHaptics");
            _foldDebug = serializedObject.FindProperty("foldDebug");

            SceneView.duringSceneGui += SyncSceneView;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= SyncSceneView;
            StopPreview();
        }

        public override void OnInspectorGUI()
        {
            InitStyles();
            serializedObject.Update();

            CameraTool tool = (CameraTool)target;

            float currentProgress = -1;
            if (_isPreviewing)
            {
                currentProgress = (float)((EditorApplication.timeSinceStartup - _previewStartTime) / _previewTotalDuration);
            }
            else if (Application.isPlaying && tool.EditorDirector && tool.EditorDirector.state == PlayState.Playing)
            {
                currentProgress = (float)(tool.EditorDirector.time / tool.EditorDirector.duration);
            }

            if (currentProgress >= 0 || _isPreviewing)
                Repaint();

            try
            {
                DrawSuiteHeader();
                DrawStatusPanel(tool);
                EditorGUILayout.Space(8);

                DrawRigSetup();
                EditorGUILayout.Space(4);

                DrawChapterWorkflow(tool);
                EditorGUILayout.Space(4);

                DrawSegmentsSection(tool, currentProgress);
                EditorGUILayout.Space(4);

                DrawLookAtAndFX(tool);
                EditorGUILayout.Space(4);

                DrawHUDSection();
                EditorGUILayout.Space(4);

                DrawUtilitiesSection(tool);
                EditorGUILayout.Space(12);
            }
            catch (ExitGUIException)
            {
                throw;
            }
            
            catch (System.Exception e)
            {
                Debug.LogException(e);
            }

            serializedObject.ApplyModifiedProperties();
        }

        // ── Section Drawing ──────────────────────────────────────────

        private void DrawRigSetup()
        {
            _foldReferences.boolValue = DrawSectionHeader("📐  Rig Setup", _foldReferences.boolValue);
            if (_foldReferences.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_splineRails, new GUIContent("Spline Rails"), true);
                EditorGUILayout.PropertyField(_targetCamera, new GUIContent("Main Camera"));
                EditorGUILayout.PropertyField(_autoHandleCamera, new GUIContent("Internal Control", "Uncheck this to release camera control for mini-games or manual TP."));
                EditorGUILayout.PropertyField(_playableDirector, new GUIContent("Master Director"));
                EditorGUI.indentLevel--;
            }
        }

        private void DrawChapterWorkflow(CameraTool tool)
        {
            _foldChapters.boolValue = DrawSectionHeader("🎞  Chapters & Sequences", _foldChapters.boolValue);
            if (_foldChapters.boolValue)
            {
                DrawChaptersSection(tool);
                DrawEventsSection();
            }
        }

        private void DrawSegmentsSection(CameraTool tool, float currentProgress)
        {
            _foldSegments.boolValue = DrawSectionHeader("⏱  Pacing & Segments", _foldSegments.boolValue);
            if (_foldSegments.boolValue)
            {
                EditorGUI.indentLevel++;
                
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(_splineProgress, new GUIContent("Manual Scrub"));
                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                    if (tool.EditorDirector != null)
                    {
                        Undo.RecordObject(tool.EditorDirector, "Manual Scrub");
                        tool.EditorDirector.time = _splineProgress.floatValue * tool.EditorDirector.duration;
                        tool.EditorDirector.Evaluate();
                    }
                }
                
                EditorGUI.indentLevel--;
                
                EditorGUILayout.Space(8);
                DrawSegmentManager(tool, currentProgress);
            }
        }

        private void DrawLookAtAndFX(CameraTool tool)
        {
            _foldAnimation.boolValue = DrawSectionHeader("🎯  Targets & Effects", _foldAnimation.boolValue);
            if (_foldAnimation.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_lookAtTarget, new GUIContent("Default LookAt"));
                EditorGUILayout.PropertyField(_lookAtTargets, new GUIContent("Target List"), true);
                EditorGUILayout.PropertyField(_lookAtWeight, new GUIContent("LookAt Weight"));
                
                DrawThinSeparator();
                DrawHapticsSection(tool);
                EditorGUI.indentLevel--;
            }
        }

        private void DrawHUDSection()
        {
            _foldViewport.boolValue = DrawSectionHeader("👁  Editor Viewport", _foldViewport.boolValue);
            if (_foldViewport.boolValue)
            {
                DrawViewportSection();
            }
        }

        private void DrawUtilitiesSection(CameraTool tool)
        {
            _foldDebug.boolValue = DrawSectionHeader("🛠  Utilities", _foldDebug.boolValue);
            if (_foldDebug.boolValue)
            {
                DrawQuickActions(tool);
                DrawDebugSection(tool);
            }
        }

        // ── Core Component View ──────────────────────────────────────

        private void DrawStatusPanel(CameraTool tool)
        {
            bool hasRails = tool.EditorRailCount > 0;
            bool hasCamera = tool.EditorCamera != null;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space(12);
            DrawStatusIndicator($"Rails: {tool.EditorRailCount}", hasRails);
            GUILayout.FlexibleSpace();
            DrawStatusIndicator("Camera", hasCamera);
            GUILayout.FlexibleSpace();
            
            bool ready = hasRails && hasCamera;
            GUILayout.Label(ready ? "● READY" : "● NOT READY", ready ? _statusOkStyle : _statusBadStyle);
            EditorGUILayout.EndHorizontal();
            DrawThinSeparator();
        }

        private void DrawStatusIndicator(string label, bool ok)
        {
            GUILayout.Label($"{(ok ? "✔" : "✘")}  {label}", ok ? _statusOkStyle : _statusBadStyle);
        }

        private void DrawChaptersSection(CameraTool tool)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space(12);
            if (GUILayout.Button("⟳  SCAN", GUILayout.Height(26))) 
                AutoScanChapters(tool);
            
            GUI.backgroundColor = CyanAccent;
            if (GUILayout.Button("⚡ AUTO-DISTRIBUTE", GUILayout.Height(26)))
            {
                Undo.RecordObject(tool, "Auto Calculate Rail Counts");
                tool.AutoCalculateRailCounts();
                EditorUtility.SetDirty(tool);
            }
            GUI.backgroundColor = Color.white;

            if (GUILayout.Button("🗑  CLEAR", GUILayout.Width(70), GUILayout.Height(26)))
            {
                if (EditorUtility.DisplayDialog("Clear Chapters", "Delete all?", "Yes", "No"))
                {
                    _chapters.arraySize = 0;
                    _selectedChapterIndex.intValue = 0;
                }
            }
            EditorGUILayout.Space(12);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(8);

            for (int i = 0; i < _chapters.arraySize; i++)
            {
                SerializedProperty chapterProp = _chapters.GetArrayElementAtIndex(i);
                SerializedProperty nameProp = chapterProp.FindPropertyRelative("name");
                SerializedProperty timelineProp = chapterProp.FindPropertyRelative("timeline");
                SerializedProperty railIdxProp = chapterProp.FindPropertyRelative("startRailIndex");
                SerializedProperty colorProp = chapterProp.FindPropertyRelative("debugColor");
                SerializedProperty isExpanded = chapterProp.FindPropertyRelative("isExpanded");

                bool isSelected = (_selectedChapterIndex.intValue == i);

                // --- Chapter Item Container ---
                EditorGUILayout.BeginVertical();
                
                // Selection highlight & line
                var rect = EditorGUILayout.BeginHorizontal(GUILayout.Height(32));
                if (isSelected)
                {
                    EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, rect.height), new Color(0.1f, 0.8f, 1f, 0.05f));
                    EditorGUI.DrawRect(new Rect(rect.x, rect.y, 2, rect.height), CyanAccent);
                }

                EditorGUILayout.Space(12);
                isExpanded.boolValue = EditorGUILayout.Foldout(isExpanded.boolValue, $"#{i}  {nameProp.stringValue}", true);

                GUILayout.FlexibleSpace();

                // Actions
                if (GUILayout.Button(isSelected ? "● FOCUSED" : "🎯 FOCUS", isSelected ? EditorStyles.miniButtonMid : EditorStyles.miniButton, GUILayout.Width(85), GUILayout.Height(20)))
                {
                    _selectedChapterIndex.intValue = i;
                    if (timelineProp.objectReferenceValue is TimelineAsset asset && tool.EditorDirector != null)
                    {
                        Undo.RecordObject(tool.EditorDirector, "Focus Chapter");
                        tool.EditorDirector.playableAsset = asset;
                        Selection.activeObject = tool.gameObject;
                        EditorApplication.ExecuteMenuItem("Window/Sequencing/Timeline");
                        TimelineEditor.Refresh(RefreshReason.ContentsModified);
                    }
                }

                GUI.backgroundColor = OkColor;
                if (GUILayout.Button("▶", GUILayout.Width(25), GUILayout.Height(20)))
                {
                    if (Application.isPlaying)
                    {
                        tool.PlayChapter(i);
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("CameraTool", "Enter Play Mode to test.", "OK");
                    }
                }
                GUI.backgroundColor = Color.white;

                if (GUILayout.Button("✕", EditorStyles.miniLabel, GUILayout.Width(20), GUILayout.Height(20)))
                {
                    _chapters.DeleteArrayElementAtIndex(i);
                    _selectedChapterIndex.intValue = Mathf.Clamp(_selectedChapterIndex.intValue, 0, _chapters.arraySize - 1);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    
                    break;
                }
                EditorGUILayout.EndHorizontal();

                // Expanded Drawer
                if (isExpanded.boolValue)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.Space(30);
                    EditorGUILayout.BeginVertical();
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(nameProp);
                    EditorGUILayout.PropertyField(timelineProp);
                    EditorGUILayout.PropertyField(railIdxProp, new GUIContent("Start Rail Index"));
                    
                    GUI.enabled = false;
                    EditorGUILayout.PropertyField(chapterProp.FindPropertyRelative("railCount"), new GUIContent("Rail Count (Auto)"));
                    GUI.enabled = true;
                    
                    EditorGUILayout.PropertyField(colorProp, new GUIContent("Debug Color"));
                    
                    if (EditorGUI.EndChangeCheck())
                    {
                        serializedObject.ApplyModifiedProperties();
                        tool.AutoCalculateRailCounts();
                        EditorUtility.SetDirty(tool);
                    }
                    
                    EditorGUILayout.Space(8);
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.EndVertical();
                DrawThinSeparator();
                EditorGUILayout.Space(2);
            }

            EditorGUILayout.Space(8);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space(12);
            if (GUILayout.Button("+ ADD MANUAL CHAPTER", GUILayout.Height(24)))
            {
                _chapters.arraySize++;
                var c = _chapters.GetArrayElementAtIndex(_chapters.arraySize - 1);
                
                c.FindPropertyRelative("name").stringValue = "New Sequence";
                c.FindPropertyRelative("isExpanded").boolValue = true;
            }
            EditorGUILayout.Space(12);
            EditorGUILayout.EndHorizontal();
        }


        private void DrawSegmentManager(CameraTool tool, float currentProgress)
        {
            if (_chapters.arraySize == 0)
                return;

            _selectedChapterIndex.intValue = Mathf.Clamp(_selectedChapterIndex.intValue, 0, _chapters.arraySize - 1);
            SerializedProperty chapter = _chapters.GetArrayElementAtIndex(_selectedChapterIndex.intValue);
            SerializedProperty segmentsProp = chapter.FindPropertyRelative("segments");

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space(12);
            GUI.enabled = false;
            
            EditorGUILayout.LabelField($"ACTIVE: {chapter.FindPropertyRelative("name").stringValue.ToUpper()}", EditorStyles.miniLabel);
            GUI.enabled = true;
            
            GUILayout.FlexibleSpace();
            
            if (GUILayout.Button("⟳ SYNC RAIL NODES", EditorStyles.miniButton, GUILayout.Width(130)))
                tool.EditorSyncSegments(_selectedChapterIndex.intValue);
            
            EditorGUILayout.Space(12);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(4);
            DrawDurationTimeline(segmentsProp, currentProgress);
            
            // Display Total Time
            float totalDur = 0;
            float totalWait = 0;
            for (int i = 0; i < segmentsProp.arraySize; i++)
            {
                var s = segmentsProp.GetArrayElementAtIndex(i);
                totalDur += s.FindPropertyRelative("duration").floatValue;
                totalWait += s.FindPropertyRelative("waitAtEnd").floatValue;
            }
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            
            var timeStyle = new GUIStyle(EditorStyles.miniLabel) { fontStyle = FontStyle.Bold, normal = { textColor = CyanAccent } };
            var waitStyle = new GUIStyle(EditorStyles.miniLabel) { fontStyle = FontStyle.Bold, normal = { textColor = WaitBarColor } };
            
            GUILayout.Label($"TOTAL DURATION: {totalDur + totalWait:F2}s", timeStyle);
            EditorGUILayout.Space(8);
            GUILayout.Label($"[ WAIT: {totalWait:F2}s ]", waitStyle);
            EditorGUILayout.Space(12);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(8);
            
            for (int i = 0; i < segmentsProp.arraySize; i++)
            {
                DrawSegmentCard(segmentsProp.GetArrayElementAtIndex(i), i, segmentsProp.arraySize);
            }

            EditorGUILayout.Space(8);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space(40);
            if (GUILayout.Button("🎬  GENERATE TIMELINE CLIPS", GUILayout.Height(30)))
                GenerateTimelineClips(tool, _selectedChapterIndex.intValue);
            
            EditorGUILayout.Space(40);
            EditorGUILayout.EndHorizontal();
        }

        private void DrawSegmentCard(SerializedProperty seg, int index, int totalCount)
        {
            EditorGUILayout.BeginHorizontal(GUILayout.Height(32));
            EditorGUILayout.Space(12);
            
            // Colorful indicator
            var dotRect = EditorGUILayout.GetControlRect(false, 20, GUILayout.Width(10));
            EditorGUI.DrawRect(new Rect(dotRect.x, dotRect.y + 6, 6, 12), Color.HSVToRGB((float)index / totalCount, 0.6f, 0.9f));

            EditorGUILayout.LabelField(seg.FindPropertyRelative("label").stringValue, EditorStyles.miniBoldLabel, GUILayout.Width(110));

            // Curve Thumbnail
            var curveRect = EditorGUILayout.GetControlRect(false, 18, GUILayout.Width(35));
            DrawMiniCurve(curveRect, seg.FindPropertyRelative("easing").animationCurveValue);
            EditorGUILayout.Space(5);

            EditorGUIUtility.labelWidth = 35;
            EditorGUILayout.PropertyField(seg.FindPropertyRelative("duration"), new GUIContent("Dur"));
            EditorGUILayout.PropertyField(seg.FindPropertyRelative("waitAtEnd"), new GUIContent("Wait"));
            EditorGUIUtility.labelWidth = 0;
            
            EditorGUILayout.Space(12);
            EditorGUILayout.EndHorizontal();
            DrawThinSeparator();
        }

        private void DrawMiniCurve(Rect rect, AnimationCurve curve)
        {
            EditorGUI.DrawRect(rect, new Color(0, 0, 0, 0.3f));
            if (curve == null)
                return;
            
            Handles.BeginGUI();
            Handles.color = CyanAccent;
            int samples = 8;
            Vector3 lastPos = new Vector3(rect.x, rect.yMax - curve.Evaluate(0) * rect.height, 0);
            
            for (int i = 1; i <= samples; i++)
            {
                float t = (float)i / samples;
                Vector3 pos = new Vector3(rect.x + t * rect.width, rect.yMax - curve.Evaluate(t) * rect.height, 0);
                Handles.DrawLine(lastPos, pos);
                lastPos = pos;
            }
            Handles.EndGUI();
        }

        private void DrawDurationTimeline(SerializedProperty segments, float progress)
        {
            float total = 0;
            for (int i = 0; i < segments.arraySize; i++)
            {
                total += segments.GetArrayElementAtIndex(i).FindPropertyRelative("duration").floatValue;
                total += segments.GetArrayElementAtIndex(i).FindPropertyRelative("waitAtEnd").floatValue;
            }
            
            if (total <= 0)
                return;

            Rect rect = EditorGUILayout.GetControlRect(false, 18);
            EditorGUI.DrawRect(rect, new Color(1,1,1, 0.05f));
            
            float x = 0;
            for (int i = 0; i < segments.arraySize; i++)
            {
                var s = segments.GetArrayElementAtIndex(i);
                float dur = s.FindPropertyRelative("duration").floatValue;
                float wait = s.FindPropertyRelative("waitAtEnd").floatValue;
                
                float w = (dur / total) * rect.width;
                EditorGUI.DrawRect(new Rect(rect.x + x, rect.y, w, rect.height), Color.HSVToRGB((float)i / segments.arraySize, 0.6f, 0.8f));
                x += w;
                
                float ww = (wait / total) * rect.width;
                EditorGUI.DrawRect(new Rect(rect.x + x, rect.y, ww, rect.height), WaitBarColor);
                x += ww;
            }

            // Draw Live Cursor
            if (progress >= 0 && progress <= 1)
            {
                float cursorX = rect.x + (progress * rect.width);
                EditorGUI.DrawRect(new Rect(cursorX - 1, rect.y - 4, 3, rect.height + 8), Color.white);
                EditorGUI.DrawRect(new Rect(cursorX - 4, rect.y - 6, 9, 3), CyanAccent);
            }
        }

        private void DrawHapticsSection(CameraTool tool)
        {
            _foldHaptics.boolValue = DrawSectionHeader("🎮  Gamepad Haptics", _foldHaptics.boolValue);
            if (!_foldHaptics.boolValue)
                return;

            if (!tool.EditorCamera)
                return;
            
            var h = tool.EditorCamera.GetComponent<CameraModifierHandler>();
            if (!h)
            {
                if (GUILayout.Button("Add Haptic Handler"))
                    tool.EditorCamera.gameObject.AddComponent<CameraModifierHandler>();
                
                return;
            }
            h.enableGamepadHaptics = EditorGUILayout.Toggle("Vibration", h.enableGamepadHaptics);
            h.hapticMultiplier = EditorGUILayout.Slider("Power", h.hapticMultiplier, 0, 2);
        }

        private void DrawViewportSection()
        {
            EditorGUI.indentLevel++;
            _showHud = EditorGUILayout.Toggle("Show HUD", _showHud);
            _showGrid = EditorGUILayout.Toggle("Rule of Thirds", _showGrid);
            _showLetterbox = EditorGUILayout.Toggle("Letterbox", _showLetterbox);
            
            if (_showLetterbox)
                _letterboxHeight = EditorGUILayout.Slider("Size", _letterboxHeight, 0.05f, 0.3f);
            
            EditorGUI.indentLevel--;
        }

        private void DrawQuickActions(CameraTool tool)
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("⏮ Start"))
                tool.EditorEvaluateAt(0);
            
            if (GUILayout.Button("⏭ End"))
                tool.EditorEvaluateAt(1);
            
            if (GUILayout.Button(_isCameraLocked ? "🔒 Locked" : "📍 Lock", _isCameraLocked ? EditorStyles.miniButtonMid : EditorStyles.miniButton))
            {
                _isCameraLocked = !_isCameraLocked;
                
                if (_isCameraLocked && tool.EditorCamera)
                    SceneView.lastActiveSceneView?.AlignViewToObject(tool.EditorCamera.transform);
            }
            if (GUILayout.Button("🎯 Snap To Selected"))
            {
                if (Selection.activeTransform)
                {
                    Undo.RecordObject(tool, "Snap to Selection");
                    tool.SnapToTransform(Selection.activeTransform);
                }
            }
            EditorGUILayout.EndHorizontal();

            if (!_isPreviewing)
            {
                GUI.backgroundColor = OkColor;
                if (GUILayout.Button("▶  Preview Animation", GUILayout.Height(28)))
                    StartPreview(tool);
            }
            else
            {
                GUI.backgroundColor = BadColor;
                if (GUILayout.Button("⏹  Stop Preview", GUILayout.Height(28)))
                    StopPreview();
            }
            
            GUI.backgroundColor = Color.white;
        }

        private void DrawDebugSection(CameraTool tool)
        {
            if (tool.EditorRailCount > 0)
            {
                var sample = tool.EditorSampleAt(_splineProgress.floatValue);
                EditorGUILayout.Vector3Field("Pos", sample.position);
            }
        }

        private void DrawEventsSection()
        {
            _foldEvents.boolValue = DrawSectionHeader("🔔  Lifecycle Events", _foldEvents.boolValue);
            if (!_foldEvents.boolValue)
                return;

            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(_onChapterStart);
            EditorGUILayout.PropertyField(_onChapterEnd);
            EditorGUI.indentLevel--;
        }

        // ── Helpers ──────────────────────────────────────────────────

        private void DrawSuiteHeader()
        {
            var rect = EditorGUILayout.GetControlRect(false, 40);
            EditorGUI.DrawRect(rect, HeaderBg);
            
            EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - 2, rect.width, 2), CyanAccent);

            var labelRect = new Rect(rect.x + 12, rect.y + 8, rect.width, 24);
            GUI.Label(labelRect, "MÉTRŌMA  |  CAMERA SUITE", _headerStyle);
            
            var versionStyle = new GUIStyle(EditorStyles.miniLabel) { alignment = TextAnchor.LowerRight, normal = { textColor = new Color(1,1,1,0.3f) } };
            GUI.Label(new Rect(rect.xMax - 70, rect.y + 18, 60, 15), "V2.0 PRO", versionStyle);
            
            EditorGUILayout.Space(8);
        }

        private static bool DrawSectionHeader(string title, bool foldout)
        {
            EditorGUILayout.Space(12);
            var rect = EditorGUILayout.GetControlRect(false, 24);
            
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, 1), LineColor);
            EditorGUI.DrawRect(new Rect(rect.x, rect.y + 6, 2, 12), CyanAccent);

            var style = new GUIStyle(EditorStyles.foldoutHeader) 
            { 
                fontSize = 12, 
                fontStyle = FontStyle.Bold,
            };
            
            return EditorGUI.Foldout(new Rect(rect.x + 10, rect.y + 4, rect.width - 10, 20), foldout, title.ToUpper(), true, style);
        }

        private static void DrawThinSeparator()
        {
            var rect = EditorGUILayout.GetControlRect(false, 1);
            EditorGUI.DrawRect(rect, LineColor);
        }

        private void InitStyles()
        {
            if (_headerStyle != null)
                return;
            
            _headerStyle = new GUIStyle(EditorStyles.label) 
            { 
                fontSize = 18, 
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };

            _statusOkStyle = new GUIStyle(EditorStyles.miniLabel) { fontSize = 10, fontStyle = FontStyle.Bold, normal = { textColor = OkColor } };
            _statusBadStyle = new GUIStyle(EditorStyles.miniLabel) { fontSize = 10, fontStyle = FontStyle.Bold, normal = { textColor = BadColor } };
        }


        // ── Logic ─────────────────────────────────────────────────────

        private void AutoScanChapters(CameraTool tool)
        {
            string[] guids = AssetDatabase.FindAssets("t:TimelineAsset");
            foreach (var guid in guids)
            {
                var asset = AssetDatabase.LoadAssetAtPath<TimelineAsset>(AssetDatabase.GUIDToAssetPath(guid));
                if (!asset.name.Contains("Camera"))
                    continue;

                bool exists = false;
                for (int i = 0; i < tool.EditorChaptersCount(); i++) 
                {
                    if (tool.EditorGetTimeline(i) == asset)
                        exists = true;
                }

                if (exists)
                    continue;

                _chapters.arraySize++;
                var c = _chapters.GetArrayElementAtIndex(_chapters.arraySize - 1);
                c.FindPropertyRelative("name").stringValue = asset.name;
                c.FindPropertyRelative("timeline").objectReferenceValue = asset;
                c.FindPropertyRelative("debugColor").colorValue = Color.HSVToRGB(Random.value, 0.7f, 0.9f);
            }
        }

        private void GenerateTimelineClips(CameraTool tool, int index)
        {
            SerializedProperty chapterProp = _chapters.GetArrayElementAtIndex(index);
            TimelineAsset timeline = chapterProp.FindPropertyRelative("timeline").objectReferenceValue as TimelineAsset;
            
            // ── 1. Automatic Timeline Creation ──
            if (!timeline)
            {
                string folder = "Assets/_Project/Runtime/Timelines/Camera";
                if (!AssetDatabase.IsValidFolder(folder))
                {
                    if (!AssetDatabase.IsValidFolder("Assets/_Project/Runtime/Timelines"))
                        AssetDatabase.CreateFolder("Assets/_Project/Runtime", "Timelines");
                        
                    AssetDatabase.CreateFolder("Assets/_Project/Runtime/Timelines", "Camera");
                }

                string cleanName = chapterProp.FindPropertyRelative("name").stringValue.Replace(" ", "_");
                string path = $"{folder}/TL-{cleanName}.playable";
                path = AssetDatabase.GenerateUniqueAssetPath(path);

                timeline = ScriptableObject.CreateInstance<TimelineAsset>();
                AssetDatabase.CreateAsset(timeline, path);
                
                chapterProp.FindPropertyRelative("timeline").objectReferenceValue = timeline;
                serializedObject.ApplyModifiedProperties();
                
                Debug.Log($"<color=#1ebfff><b>[CameraTool]</b></color> Generated new Timeline: {path}");
            }
            if (!timeline)
                return;

            // ── 2. Track & Binding Logic ──
            Undo.RecordObject(timeline, "Generate Camera Timeline");
            
            CameraToolTrack track = null;
            foreach (var t in timeline.GetOutputTracks())
            {
                if (t is CameraToolTrack ct)
                {
                    track = ct;
                    break;
                }
            }

            if (!track)
                track = timeline.CreateTrack<CameraToolTrack>(null, "Camera Track");

            // ── 3. Clean and Populate Clips ──
            var existingClips = new List<TimelineClip>(track.GetClips());
            foreach (var clip in existingClips)
            {
                if (clip.asset != null)
                    AssetDatabase.RemoveObjectFromAsset(clip.asset);

                track.DeleteClip(clip);
            }

            var segs = tool.EditorSegments(index);
            if (segs.Count == 0)
            {
                return;
            }

            // Calculate total MOVE chapter duration to correctly map spline progress (0-1)
            float totalMoveDur = 0f;
            foreach (var s in segs)
            {
                totalMoveDur += s.duration;
            }

            int startRailIdx = tool.EditorChapter(index).startRailIndex;
            int railLimit = Mathf.Min(startRailIdx + tool.EditorChapter(index).railCount, tool.EditorRailCount);
            
            // --- Generation: ONE CLIP PER RAIL ---
            double clipTimelineStart = 0;
            float accumulatedMoveDur = 0f;

            for (int r = startRailIdx; r < railLimit; r++)
            {
                int segmentsInThisRail = tool.EditorSegmentCountInRail(r);
                if (segmentsInThisRail <= 0)
                {
                    continue;
                }

                TimelineClip clip = track.CreateDefaultClip();
                clip.displayName = $"Rail {r} Sequence";
                clip.start = clipTimelineStart;

                // 1. Duration on Timeline (includes waits)
                float railTimelineDuration = 0;
                // 2. Duration for Spline Progress (move only)
                float railMoveDuration = 0;

                int segStartIdx = 0;
                // Important: find where this rail starts in the chapter's flat segment list
                for (int prev = startRailIdx; prev < r; prev++)
                {
                    segStartIdx += tool.EditorSegmentCountInRail(prev);
                }

                for (int s = 0; s < segmentsInThisRail; s++)
                {
                    if (segStartIdx + s < segs.Count)
                    {
                        var seg = segs[segStartIdx + s];
                        railTimelineDuration += seg.duration + seg.waitAtEnd;
                        railMoveDuration += seg.duration;
                    }
                }

                clip.duration = railTimelineDuration;

                CameraToolClip asset = clip.asset as CameraToolClip;
                if (asset)
                {
                    asset.name = $"Rail_{r}_Sequence_Clip";
                    
                    if (!EditorUtility.IsPersistent(asset))
                    {
                        AssetDatabase.AddObjectToAsset(asset, timeline);
                    }

                    // Map progress relative to the Chapter using MOVE durations
                    asset.Template.railIndex = r;
                    asset.Template.chapterIndex = index;

                    if (totalMoveDur > 0)
                    {
                        asset.Template.startProgress = accumulatedMoveDur / totalMoveDur;
                        asset.Template.endProgress = (accumulatedMoveDur + railMoveDuration) / totalMoveDur;
                    }
                    else
                    {
                        asset.Template.startProgress = 0;
                        asset.Template.endProgress = 1;
                    }
                    asset.Template.easingCurve = AnimationCurve.Linear(0, 0, 1, 1);
                }

                clipTimelineStart += railTimelineDuration;
                accumulatedMoveDur += railMoveDuration;
            }

            // ── 4. Automatic binding to Director ──
            if (tool.EditorDirector)
            {
                Undo.RecordObject(tool.EditorDirector, "Bind Camera Track");
                tool.EditorDirector.SetGenericBinding(track, tool);
            }

            // ── 5. Persistence & Live Refresh ──
            EditorUtility.SetDirty(timeline);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            TimelineEditor.Refresh(RefreshReason.ContentsModified | RefreshReason.ContentsAddedOrRemoved);

            if (TimelineEditor.inspectedDirector && TimelineEditor.inspectedAsset == timeline)
            {
                TimelineEditor.inspectedDirector.RebuildGraph();
            }
            
            Debug.Log($"<color=#1ebfff><b>[CameraTool]</b></color> Timeline synced successfully with {segs.Count} segments.");
        }

        private void StartPreview(CameraTool tool)
        {
            var chapter = _chapters.GetArrayElementAtIndex(_selectedChapterIndex.intValue);
            var segs = chapter.FindPropertyRelative("segments");
            if (segs.arraySize == 0)
                return;

            _previewSegStarts = new float[segs.arraySize];
            _previewSegDurations = new float[segs.arraySize];
            _previewSegCurves = new AnimationCurve[segs.arraySize];

            float time = 0;
            for (int i = 0; i < segs.arraySize; i++)
            {
                var s = segs.GetArrayElementAtIndex(i);
                _previewSegStarts[i] = time;
                _previewSegDurations[i] = s.FindPropertyRelative("duration").floatValue;
                _previewSegCurves[i] = s.FindPropertyRelative("easing").animationCurveValue;
                time += _previewSegDurations[i] + s.FindPropertyRelative("waitAtEnd").floatValue;
            }

            _previewTotalDuration = time;
            _previewStartTime = EditorApplication.timeSinceStartup;
            _isPreviewing = true;
            EditorApplication.update += PreviewUpdate;
        }

        private void StopPreview()
        {
            if (!_isPreviewing)
                return;
            
            _isPreviewing = false; EditorApplication.update -= PreviewUpdate;
        }

        private void PreviewUpdate()
        {
            if (!_isPreviewing || target == null)
            {
                StopPreview();
                return;
            }
            
            float elapsed = (float)(EditorApplication.timeSinceStartup - _previewStartTime);
            if (elapsed >= _previewTotalDuration)
            {
                ((CameraTool)target).EditorEvaluateAt(1f);
                StopPreview();
                return;
            }
            
            int count = _previewSegStarts.Length;
            float p = 0;
            for (int i = 0; i < count; i++)
            {
                if (elapsed < _previewSegStarts[i] + _previewSegDurations[i])
                {
                    float t = _previewSegCurves[i].Evaluate((elapsed - _previewSegStarts[i]) / _previewSegDurations[i]);
                    p = Mathf.Lerp((float)i / count, (float)(i + 1) / count, t);
                    break;
                }
            }
            ((CameraTool)target).EditorEvaluateAt(p);
            SceneView.RepaintAll();
        }

        private void SyncSceneView(SceneView sv)
        {
            if (!_isCameraLocked || Application.isPlaying)
                return;
            
            var cam = ((CameraTool)target).EditorCamera;
            if (cam == null)
                return;
            
            sv.pivot = cam.transform.position; sv.rotation = cam.transform.rotation; sv.size = 0;
            sv.Repaint();
        }
    }
}
