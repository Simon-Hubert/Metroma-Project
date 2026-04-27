using UnityEngine;
using UnityEditor;
using Metroma.CameraTool;
using Metroma.CameraTool.Timeline;
using Metroma.CameraTool.Modules;
using Dreamteck.Splines;
using UnityEngine.Timeline;
using UnityEngine.Playables;
using System.Collections.Generic;
using Metroma.CameraTool.Modifiers;
using UnityEditor.Timeline;

namespace Metroma.CameraTool.Editor
{
    [CustomEditor(typeof(CameraRig))]
    public class CameraToolEditor : UnityEditor.Editor
    {
        #region --- Fields & Properties ---

        private SerializedProperty _targetCamera;
        private SerializedProperty _autoHandleTransform;
        private SerializedProperty _onSplineNotified;

        private SerializedProperty _selectedChapterIndex;
        private SerializedProperty _foldReferences;
        private SerializedProperty _foldChapters;
        private SerializedProperty _foldSegments;
        private SerializedProperty _foldAnimation;
        private SerializedProperty _foldEvents;
        private SerializedProperty _foldViewport;
        private SerializedProperty _foldHaptics;
        private SerializedProperty _foldDebug;

        private SerializedObject _railSerialized;
        private SerializedObject _sequenceSerialized;

        private SerializedProperty _splineRails;
        private SerializedProperty _playableDirector;
        private SerializedProperty _lookAtTarget;
        private SerializedProperty _lookAtTargets;
        private SerializedProperty _globalProgress;
        private SerializedProperty _lookAtWeight;
        private SerializedProperty _defaultFOV;
        private SerializedProperty _chapters;
        private SerializedProperty _playOnStart;
        private SerializedProperty _onChapterStart;
        private SerializedProperty _onChapterEnd;

        private SerializedProperty _debugFocusTimeline;
        private SerializedProperty _debugBlendIn;
        private SerializedProperty _debugBlendOut;

        private bool _isCameraLocked;
        private bool _showHud;
        private bool _showGrid = true;
        private bool _showLetterbox = true;
        private float _letterboxHeight = 0.12f;

        private bool _isPreviewing;
        private double _previewStartTime;
        private float _previewTotalDuration;
        private float[] _previewSegStarts;
        private float[] _previewSegDurations;
        private AnimationCurve[] _previewSegCurves;

        private static GUIStyle _headerStyle;
        private static GUIStyle _statusOkStyle;
        private static GUIStyle _statusBadStyle;

        private static readonly Color CyanAccent = new Color(0.1f, 0.8f, 1f);
        private static readonly Color HeaderBg = new Color(0.08f, 0.08f, 0.08f, 1f);
        private static readonly Color OkColor = new Color(0.2f, 0.85f, 0.4f);
        private static readonly Color BadColor = new Color(1f, 0.3f, 0.4f);
        private static readonly Color WaitBarColor = new Color(1f, 0.6f, 0.1f, 0.4f);
        private static readonly Color LineColor = new Color(1f, 1f, 1f, 0.08f);

        #endregion

        #region --- Lifecycle ---

        private void OnEnable()
        {
            if (target == null) return;

            // Rig Base
            _targetCamera = serializedObject.FindProperty("targetCamera");
            _autoHandleTransform = serializedObject.FindProperty("autoHandleTransform");
            _onSplineNotified = serializedObject.FindProperty("onSplineNotified");

            _selectedChapterIndex = serializedObject.FindProperty("selectedChapterIndex");
            _foldReferences = serializedObject.FindProperty("foldReferences");
            _foldChapters = serializedObject.FindProperty("foldChapters");
            _foldSegments = serializedObject.FindProperty("foldSegments");
            _foldAnimation = serializedObject.FindProperty("foldAnimation");
            _foldEvents = serializedObject.FindProperty("foldEvents");
            _foldViewport = serializedObject.FindProperty("foldViewport");
            _foldHaptics = serializedObject.FindProperty("foldHaptics");
            _foldDebug = serializedObject.FindProperty("foldDebug");

            RefreshModuleBindings();
            SceneView.duringSceneGui += SyncSceneView;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= SyncSceneView;
            StopPreview();
        }

        private void RefreshModuleBindings()
        {
            CameraRig rig = (CameraRig)target;
            
            var railModule = rig.Rails;
            if (railModule != null)
            {
                _railSerialized = new SerializedObject(railModule);
                _splineRails = _railSerialized.FindProperty("splineRails");
                _lookAtTarget = _railSerialized.FindProperty("lookAtTarget");
                _lookAtTargets = _railSerialized.FindProperty("lookAtTargets");
                _globalProgress = _railSerialized.FindProperty("globalProgress");
                _lookAtWeight = _railSerialized.FindProperty("lookAtWeight");
                _defaultFOV = _railSerialized.FindProperty("defaultFOV");
            }

            var sequenceModule = rig.Sequences;
            if (sequenceModule != null)
            {
                _sequenceSerialized = new SerializedObject(sequenceModule);
                _chapters = _sequenceSerialized.FindProperty("chapters");
                _playOnStart = _sequenceSerialized.FindProperty("playOnStart");
                _playableDirector = _sequenceSerialized.FindProperty("playableDirector");
                _onChapterStart = _sequenceSerialized.FindProperty("onChapterStart");
                _onChapterEnd = _sequenceSerialized.FindProperty("onChapterEnd");
                _debugFocusTimeline = _sequenceSerialized.FindProperty("debugFocusTimeline");
                _debugBlendIn = _sequenceSerialized.FindProperty("debugBlendIn");
                _debugBlendOut = _sequenceSerialized.FindProperty("debugBlendOut");
            }
        }

        #endregion

        #region --- Main UI ---

        public override void OnInspectorGUI()
        {
            if (target == null) return;

            InitStyles();
            serializedObject.Update();

            if (_railSerialized != null) _railSerialized.Update();
            else RefreshModuleBindings();

            if (_sequenceSerialized != null) _sequenceSerialized.Update();
            else RefreshModuleBindings();

            CameraRig rig = (CameraRig)target;

            // Paint Loop Management
            float currentProgress = -1;
            if (_isPreviewing)
                currentProgress = (float)((EditorApplication.timeSinceStartup - _previewStartTime) / _previewTotalDuration);
            else if (Application.isPlaying && rig.Sequences != null && rig.Sequences.Director != null && rig.Sequences.Director.state == PlayState.Playing)
                currentProgress = (float)(rig.Sequences.Director.time / rig.Sequences.Director.duration);

            if (currentProgress >= 0 || _isPreviewing)
                Repaint();

            try
            {
                DrawSuiteHeader();
                DrawStatusPanel(rig);
                EditorGUILayout.Space(8);

                DrawSection("📐  RIG SETUP", _foldReferences, DrawRigSetupFields);
                DrawSection("🎞  CHAPTERS & SEQUENCES", _foldChapters, () => DrawChapterWorkflow(rig));
                DrawSection("⏱  PACING & SEGMENTS", _foldSegments, () => DrawSegmentsSection(rig, currentProgress));
                DrawSection("🎯  TARGETS & EFFECTS", _foldAnimation, () => DrawLookAtAndFX(rig));
                DrawSection("👁  EDITOR VIEWPORT", _foldViewport, DrawViewportSection);
                DrawSection("🛠  UTILITIES", _foldDebug, () => DrawUtilitiesSection(rig));
                
                EditorGUILayout.Space(12);
            }
            catch (ExitGUIException) { throw; }
            catch (System.Exception e) { Debug.LogException(e); }

            // Apply all changes
            serializedObject.ApplyModifiedProperties();

            if (_railSerialized != null && _railSerialized.ApplyModifiedProperties())
            {
                CameraToolUtility.AutoCalculateRailCounts(rig);
                if (_sequenceSerialized != null) _sequenceSerialized.Update();
            }

            if (_sequenceSerialized != null && _sequenceSerialized.ApplyModifiedProperties())
            {
                CameraToolUtility.AutoCalculateRailCounts(rig);
                _sequenceSerialized.Update();
            }
        }

        #endregion

        #region --- Sections ---

        private void DrawSection(string title, SerializedProperty foldoutProp, System.Action drawFunc)
        {
            foldoutProp.boolValue = DrawSectionHeader(title, foldoutProp.boolValue);
            if (foldoutProp.boolValue)
            {
                drawFunc?.Invoke();
                EditorGUILayout.Space(4);
            }
        }

        private void DrawRigSetupFields()
        {
            EditorGUI.indentLevel++;
            if (_splineRails != null)
                EditorGUILayout.PropertyField(_splineRails, new GUIContent("Spline Rails"), true);
            
            EditorGUILayout.PropertyField(_targetCamera, new GUIContent("Main Camera"));
            EditorGUILayout.PropertyField(_autoHandleTransform, new GUIContent("Internal Control", "Uncheck to release camera handle."));
            if (_playableDirector != null)
                EditorGUILayout.PropertyField(_playableDirector, new GUIContent("Master Director"));

            EditorGUI.indentLevel--;
        }

        private void DrawChapterWorkflow(CameraRig rig)
        {
            if (_playOnStart != null)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_playOnStart, new GUIContent("🚀  Auto-Play On Start", "If checked, the first chapter will launch automatically when entering Play Mode."));
                EditorGUI.indentLevel--;
                EditorGUILayout.Space(5);
            }

            DrawChaptersSection(rig);
            
            EditorGUILayout.Space(4);
            _foldEvents.boolValue = DrawSectionHeader("🔔  Lifecycle Events", _foldEvents.boolValue);
            if (_foldEvents.boolValue)
            {
                EditorGUI.indentLevel++;
                if (_onChapterStart != null)
                    EditorGUILayout.PropertyField(_onChapterStart);
                
                if (_onChapterEnd != null)
                    EditorGUILayout.PropertyField(_onChapterEnd);
                
                EditorGUILayout.PropertyField(_onSplineNotified);
                EditorGUI.indentLevel--;
            }
        }

        private void DrawSegmentsSection(CameraRig rig, float currentProgress)
        {
            EditorGUI.indentLevel++;
            EditorGUI.BeginChangeCheck();

            float normalizedTime = 0f;
            bool hasTimeline = rig.Sequences && rig.Sequences.Director && rig.Sequences.Director.playableAsset;

            if (hasTimeline && rig.Sequences.Director.duration > 0)
            {
                normalizedTime = (float)(rig.Sequences.Director.time / rig.Sequences.Director.duration);
            }
            else if (_globalProgress != null)
            {
                normalizedTime = _globalProgress.floatValue;
            }

            normalizedTime = EditorGUILayout.Slider("Manual Scrub", normalizedTime, 0f, 1f);

            if (EditorGUI.EndChangeCheck())
            {
                if (hasTimeline)
                {
                    Undo.RecordObject(rig.Sequences.Director, "Manual Scrub");
        
                    rig.Sequences.Director.time = normalizedTime * rig.Sequences.Director.duration;
                    rig.Sequences.Director.Evaluate();
        
                    TimelineEditor.Refresh(RefreshReason.WindowNeedsRedraw);
                }
                else if (_globalProgress != null)
                {
                    _globalProgress.floatValue = normalizedTime;
                    _railSerialized?.ApplyModifiedProperties();

                    rig.EditorReportVisualState(-1, -1, normalizedTime);
                }
    
                SceneView.RepaintAll();
            }

            EditorGUI.indentLevel--;
            EditorGUILayout.Space(8);
            DrawSegmentManager(rig, currentProgress);
        }

        private void DrawLookAtAndFX(CameraRig rig)
        {
            EditorGUI.indentLevel++;
            if (_lookAtTarget != null)
                EditorGUILayout.PropertyField(_lookAtTarget, new GUIContent("Default LookAt"));
            
            if (_lookAtTargets != null)
                EditorGUILayout.PropertyField(_lookAtTargets, new GUIContent("Target List"), true);
            
            if (_lookAtWeight != null)
                EditorGUILayout.PropertyField(_lookAtWeight, new GUIContent("LookAt Weight"));
            
            if (_defaultFOV != null)
                EditorGUILayout.PropertyField(_defaultFOV, new GUIContent("Default FOV"));
            
            DrawThinSeparator();
            DrawHapticsSection(rig);
            EditorGUI.indentLevel--;
        }

        #endregion

        #region --- Sub Components ---

        private void DrawStatusPanel(CameraRig rig)
        {
            bool hasRails = rig.Rails && rig.Rails.RailCount > 0;
            bool hasCamera = rig.TargetCamera;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space(12);
            DrawStatusIndicator($"Rails: {(rig.Rails ? rig.Rails.RailCount : 0)}", hasRails);
            GUILayout.FlexibleSpace();
            DrawStatusIndicator("Camera", hasCamera);
            GUILayout.FlexibleSpace();
            
            bool ready = hasRails && hasCamera;
            if (!ready)
            {
                GUI.backgroundColor = CyanAccent;
                if (GUILayout.Button("⚡ INITIALIZE RIG", GUILayout.Width(130), GUILayout.Height(20)))
                    CameraToolUtility.FullSetupRig(rig);
                
                GUI.backgroundColor = Color.white;
            }
            else
            {
                GUILayout.Label("● READY", _statusOkStyle);
            }
            EditorGUILayout.EndHorizontal();
            DrawThinSeparator();
        }

        private void DrawStatusIndicator(string label, bool ok)
        {
            GUILayout.Label($"{(ok ? "✔" : "✘")}  {label}", ok ? _statusOkStyle : _statusBadStyle);
        }

        private void DrawChaptersSection(CameraRig rig)
        {
            if (_chapters == null)
                return;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space(12);
            if (GUILayout.Button("⟳  SCAN", GUILayout.Height(26))) 
            {
                CameraToolUtility.AutoScanChapters(rig);
                
                if (_sequenceSerialized != null)
                    _sequenceSerialized.Update();
            }

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
                SerializedProperty railCountProp = chapterProp.FindPropertyRelative("railCount");
                SerializedProperty waitAtStartProp = chapterProp.FindPropertyRelative("waitAtStart");
                SerializedProperty isExpanded = chapterProp.FindPropertyRelative("isExpanded");

                bool isSelected = (_selectedChapterIndex.intValue == i);
                int endRail = railIdxProp.intValue + railCountProp.intValue - 1;

                string labelText = $"#{i:00}  {nameProp.stringValue.ToUpper()}";
                string detailsText = ($"  (Rails {railIdxProp.intValue} → {endRail})").ToUpper();

                EditorGUILayout.BeginVertical();
                var rect = EditorGUILayout.BeginHorizontal(GUILayout.Height(36));
                
                EditorGUI.DrawRect(new Rect(rect.x + 8, rect.y, rect.width - 16, rect.height), new Color(0, 0, 0, 0.2f));
                if (isSelected)
                {
                    EditorGUI.DrawRect(new Rect(rect.x + 8, rect.y, rect.width - 16, rect.height), new Color(0.1f, 0.8f, 1f, 0.08f));
                    EditorGUI.DrawRect(new Rect(rect.x + 8, rect.y, 3, rect.height), CyanAccent);
                }

                // --- 1. Pure Manual Header Part ---
                isExpanded.boolValue = GUI.Toggle(new Rect(rect.x + 20, rect.y + 11, 16, 16), isExpanded.boolValue, GUIContent.none, EditorStyles.foldout);
                
                GUI.Label(new Rect(rect.x + 42, rect.y + 8, 20, 20), EditorGUIUtility.IconContent("TimelineAsset Icon"));
                
                var labelStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 11, alignment = TextAnchor.MiddleLeft, clipping = TextClipping.Clip };
                GUI.Label(new Rect(rect.x + 64, rect.y + 8, rect.width - 200, 20), labelText, labelStyle);
                
                var detailStyle = new GUIStyle(EditorStyles.miniLabel) { normal = { textColor = new Color(1, 1, 1, 0.4f) }, fontSize = 9 };
                GUI.Label(new Rect(rect.x + 42, rect.y + 22, 150, 14), detailsText, detailStyle);

                GUILayout.FlexibleSpace();
                
                // --- 2. Right Actions Row ---
                EditorGUILayout.BeginVertical();
                EditorGUILayout.Space(8);
                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button(isSelected ? "● EDIT" : "🎬 FOCUS", isSelected ? EditorStyles.miniButtonMid : EditorStyles.miniButton, GUILayout.Width(65), GUILayout.Height(20)))
                {
                    _selectedChapterIndex.intValue = i;
                    if (timelineProp.objectReferenceValue is TimelineAsset asset && rig.Sequences?.Director != null)
                    {
                        rig.Sequences.Director.playableAsset = asset;
                        Selection.activeObject = rig.gameObject;
                        EditorApplication.ExecuteMenuItem("Window/Sequencing/Timeline");
                    }
                }

                GUI.backgroundColor = OkColor;
                if (GUILayout.Button(EditorGUIUtility.IconContent("PlayButton"), GUILayout.Width(26), GUILayout.Height(20)))
                    if (Application.isPlaying && rig.Sequences)
                        rig.Sequences.PlayChapter(i);
                
                GUI.backgroundColor = Color.white;

                if (GUILayout.Button(EditorGUIUtility.IconContent("TreeEditor.Trash"), EditorStyles.miniLabel, GUILayout.Width(22), GUILayout.Height(20)))
                {
                    _chapters.DeleteArrayElementAtIndex(i);
                    _selectedChapterIndex.intValue = Mathf.Clamp(_selectedChapterIndex.intValue, 0, _chapters.arraySize - 1);
                    
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    break;
                }
                
                EditorGUILayout.Space(8);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();

                EditorGUILayout.EndHorizontal();

                // --- 3. Expanded Body ---
                if (isExpanded.boolValue)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.Space(40);
                    EditorGUILayout.BeginVertical();
                    EditorGUILayout.PropertyField(nameProp);
                    EditorGUILayout.PropertyField(timelineProp);
                    EditorGUILayout.PropertyField(railIdxProp, new GUIContent("Start Rail Index"));
                    GUI.enabled = false;
                    
                    EditorGUILayout.PropertyField(railCountProp, new GUIContent("Rail Count (Calculated)"));
                    GUI.enabled = true;
                    
                    EditorGUILayout.PropertyField(waitAtStartProp);
                    
                    EditorGUILayout.Space(12);
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space(12);
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(4);
            }
            EditorGUILayout.Space(4);
            if (GUILayout.Button("+ ADD MANUAL CHAPTER", GUILayout.Height(24))) _chapters.arraySize++;
        }

        private void DrawSegmentManager(CameraRig rig, float currentProgress)
        {
            if (_chapters == null || _chapters.arraySize == 0)
                return;

            _selectedChapterIndex.intValue = Mathf.Clamp(_selectedChapterIndex.intValue, 0, _chapters.arraySize - 1);
            var chapter = _chapters.GetArrayElementAtIndex(_selectedChapterIndex.intValue);
            var segmentsProp = chapter.FindPropertyRelative("segments");

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space(12);
            GUI.enabled = false;
            EditorGUILayout.LabelField($"ACTIVE: {chapter.FindPropertyRelative("name").stringValue.ToUpper()}", EditorStyles.miniLabel);
            GUI.enabled = true;
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("⟳ SYNC RAIL NODES", EditorStyles.miniButton, GUILayout.Width(130)))
                CameraToolUtility.SyncChapterSegments(rig, _selectedChapterIndex.intValue);
            
            EditorGUILayout.Space(12);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(4);
            DrawDurationTimeline(segmentsProp, currentProgress);
            
            float totalDur = 0; 
            for (int i = 0; i < segmentsProp.arraySize; i++) 
            {
                var s = segmentsProp.GetArrayElementAtIndex(i);
                totalDur += s.FindPropertyRelative("duration").floatValue + s.FindPropertyRelative("waitAtEnd").floatValue;
            }
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            var timeStyle = new GUIStyle(EditorStyles.miniLabel) { fontStyle = FontStyle.Bold, normal = { textColor = CyanAccent } };
            GUILayout.Label($"TOTAL DURATION: {totalDur:F2}s", timeStyle);
            EditorGUILayout.Space(12);
            EditorGUILayout.EndHorizontal();

            for (int i = 0; i < segmentsProp.arraySize; i++)
            {
                // DETECT RAIL JUNCTION
                // If this segment belongs to a different rail than the previous one
                if (i > 0)
                {
                    // Logic to find which rail this segment belongs to
                    // This is slightly complex because segments are nodes.
                    // But we can check the Rail Indices from the EditorRails
                    int segmentsAcc = 0;
                    int currentRailIdx = -1;
                    int prevRailIdx = -1;

                    int startRail = chapter.FindPropertyRelative("startRailIndex").intValue;
                    int railCountInChapter = chapter.FindPropertyRelative("railCount").intValue;

                    for (int r = startRail; r < startRail + railCountInChapter; r++)
                    {
                        if (r >= rig.Rails.EditorRails.Count) break;
                        var rail = rig.Rails.EditorRails[r];
                        if (!rail) continue;
                        
                        int railSegs = Mathf.Max(0, rail.pointCount - 1);
                        
                        if (i >= segmentsAcc && i < segmentsAcc + railSegs)
                        {
                            currentRailIdx = r;
                        }
                        if ((i-1) >= segmentsAcc && (i-1) < segmentsAcc + railSegs)
                        {
                            prevRailIdx = r;
                        }
                        segmentsAcc += railSegs;
                    }

                    if (currentRailIdx != prevRailIdx && currentRailIdx != -1 && prevRailIdx != -1)
                    {
                        var junctionSegIdx = i - 1;
                        var junctionSegProp = segmentsProp.GetArrayElementAtIndex(junctionSegIdx);
                        var overrideProp = junctionSegProp.FindPropertyRelative("junctionOverride");
                        var isExp = junctionSegProp.FindPropertyRelative("isExpanded");

                        EditorGUILayout.Space(6);
                        
                        // Panel Background
                        var rect = EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, 24), new Color(1, 0.6f, 0.1f, 0.15f));
                        
                        // Header
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(4);
                        var junctionStyle = new GUIStyle(EditorStyles.miniLabel) { fontStyle = FontStyle.Bold, normal = { textColor = new Color(1, 0.6f, 0.1f) } };
                        GUILayout.Label($"▲ RAIL JUNCTION BLEND (Local Override)", junctionStyle);
                        GUILayout.FlexibleSpace();
                        
                        if (GUILayout.Button(isExp.boolValue ? "▼" : "◀", EditorStyles.miniLabel, GUILayout.Width(20)))
                        {
                            isExp.boolValue = !isExp.boolValue;
                        }
                        
                        EditorGUILayout.EndHorizontal();

                        if (isExp.boolValue)
                        {
                            EditorGUI.indentLevel++;
                            var durProp = overrideProp.FindPropertyRelative("duration");
                            var distProp = overrideProp.FindPropertyRelative("blendDistance");
                            var smoothProp = overrideProp.FindPropertyRelative("smoothness");

                            EditorGUILayout.PropertyField(durProp, new GUIContent("Duration (s)", durProp.tooltip));
                            EditorGUILayout.PropertyField(distProp, new GUIContent("Blend Distance (m)", distProp.tooltip));
                            EditorGUILayout.PropertyField(smoothProp, new GUIContent("Smoothness", smoothProp.tooltip));
                            EditorGUI.indentLevel--;
                            EditorGUILayout.Space(4);
                        }

                        EditorGUILayout.EndVertical();
                        EditorGUILayout.Space(8);
                    }
                }

                DrawSegmentCard(segmentsProp.GetArrayElementAtIndex(i), i, segmentsProp.arraySize);
            }

            EditorGUILayout.Space(8);
            if (GUILayout.Button("🎬  GENERATE TIMELINE CLIPS", GUILayout.Height(30)))
                GenerateTimelineClips(rig, _selectedChapterIndex.intValue);
        }

        private void DrawSegmentCard(SerializedProperty seg, int index, int totalCount)
        {
            EditorGUILayout.BeginHorizontal(GUILayout.Height(32));
            EditorGUILayout.Space(12);
            var dotRect = EditorGUILayout.GetControlRect(false, 20, GUILayout.Width(10));
            EditorGUI.DrawRect(new Rect(dotRect.x, dotRect.y + 6, 6, 12), Color.HSVToRGB((float)index / totalCount, 0.6f, 0.9f));
            EditorGUILayout.LabelField(seg.FindPropertyRelative("label").stringValue, EditorStyles.miniBoldLabel, GUILayout.Width(130));
            
            var curveRect = EditorGUILayout.GetControlRect(false, 18, GUILayout.Width(35));
            DrawMiniCurve(curveRect, seg.FindPropertyRelative("easing").animationCurveValue);
            
            EditorGUIUtility.labelWidth = 35;
            EditorGUILayout.PropertyField(seg.FindPropertyRelative("duration"), new GUIContent("Dur"));
            EditorGUILayout.PropertyField(seg.FindPropertyRelative("waitAtEnd"), new GUIContent("Wait"));
            EditorGUIUtility.labelWidth = 0;
            EditorGUILayout.EndHorizontal();
            DrawThinSeparator();
        }

        #endregion

        #region --- Visual Helpers ---

        private void DrawMiniCurve(Rect rect, AnimationCurve curve)
        {
            EditorGUI.DrawRect(rect, new Color(0, 0, 0, 0.3f));
            if (curve == null) return;
            Handles.BeginGUI(); Handles.color = CyanAccent;
            int samples = 8; Vector3 lastPos = new Vector3(rect.x, rect.yMax - curve.Evaluate(0) * rect.height, 0);
            for (int i = 1; i <= samples; i++) {
                float t = (float)i / samples;
                Vector3 pos = new Vector3(rect.x + t * rect.width, rect.yMax - curve.Evaluate(t) * rect.height, 0);
                Handles.DrawLine(lastPos, pos); lastPos = pos;
            }
            Handles.EndGUI();
        }

        private void DrawDurationTimeline(SerializedProperty segments, float progress)
        {
            float total = 1e-5f;
            if (segments == null) return;
            for (int i = 0; i < segments.arraySize; i++)
                total += segments.GetArrayElementAtIndex(i).FindPropertyRelative("duration").floatValue + segments.GetArrayElementAtIndex(i).FindPropertyRelative("waitAtEnd").floatValue;

            Rect rect = EditorGUILayout.GetControlRect(false, 18);
            EditorGUI.DrawRect(rect, new Color(1,1,1, 0.05f));
            float x = 0;
            for (int i = 0; i < segments.arraySize; i++) {
                var s = segments.GetArrayElementAtIndex(i);
                float dur = s.FindPropertyRelative("duration").floatValue; float wait = s.FindPropertyRelative("waitAtEnd").floatValue;
                float w = (dur / total) * rect.width;
                EditorGUI.DrawRect(new Rect(rect.x + x, rect.y, w, rect.height), Color.HSVToRGB((float)i / segments.arraySize, 0.6f, 0.8f));
                x += w;
                float ww = (wait / total) * rect.width;
                EditorGUI.DrawRect(new Rect(rect.x + x, rect.y, ww, rect.height), WaitBarColor);
                x += ww;
            }
            if (progress >= 0 && progress <= 1)
                EditorGUI.DrawRect(new Rect(rect.x + (progress * rect.width) - 1, rect.y - 4, 3, rect.height + 8), Color.white);
        }

        private void DrawHapticsSection(CameraRig rig)
        {
            _foldHaptics.boolValue = DrawSectionHeader("🎮  Gamepad Haptics", _foldHaptics.boolValue);
            if (!_foldHaptics.boolValue || rig.TargetCamera == null) return;
            var h = rig.TargetCamera.GetComponent<CameraModifierHandler>();
            if (!h) return;
            EditorGUI.indentLevel++;
            h.enableGamepadHaptics = EditorGUILayout.Toggle("Vibration", h.enableGamepadHaptics);
            h.hapticMultiplier = EditorGUILayout.Slider("Power", h.hapticMultiplier, 0, 2);
            EditorGUI.indentLevel--;
        }

        private void DrawViewportSection()
        {
            EditorGUI.indentLevel++;
            _showHud = EditorGUILayout.Toggle("Show HUD", _showHud);
            _showGrid = EditorGUILayout.Toggle("Rule of Thirds", _showGrid);
            _showLetterbox = EditorGUILayout.Toggle("Letterbox", _showLetterbox);
            if (_showLetterbox) _letterboxHeight = EditorGUILayout.Slider("Size", _letterboxHeight, 0.05f, 0.3f);
            EditorGUI.indentLevel--;
        }

        private void DrawQuickActions(CameraRig rig)
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("⏮ Start")) rig.EditorReportVisualState(-1, -1, 0);
            if (GUILayout.Button("⏭ End")) rig.EditorReportVisualState(-1, -1, 1);
            if (GUILayout.Button(_isCameraLocked ? "🔒 Locked" : "📍 Lock")) {
                _isCameraLocked = !_isCameraLocked;
                if (_isCameraLocked && rig.TargetCamera) SceneView.lastActiveSceneView?.AlignViewToObject(rig.TargetCamera.transform);
            }
            if (GUILayout.Button("🎯 Snap To Selected") && Selection.activeTransform) {
                Undo.RecordObject(rig, "Snap to Selection");
                rig.SnapToTransform(Selection.activeTransform);
            }
            EditorGUILayout.EndHorizontal();

            if (!_isPreviewing) {
                GUI.backgroundColor = OkColor;
                if (GUILayout.Button("▶  Preview Animation", GUILayout.Height(28))) StartPreview(rig);
            } else {
                GUI.backgroundColor = BadColor;
                if (GUILayout.Button("⏹  Stop Preview", GUILayout.Height(28))) StopPreview();
            }
            GUI.backgroundColor = Color.white;
        }

        private void DrawDebugSection(CameraRig rig)
        {
            if (rig.Rails != null && rig.Rails.RailCount > 0) {
                var sample = rig.Rails.CalculateTargetPose();
                EditorGUILayout.Vector3Field("Current Pos", sample.position);
            }
        }

        private void DrawUtilitiesSection(CameraRig rig)
        {
            DrawQuickActions(rig);
            DrawDebugSection(rig);

            EditorGUILayout.Space(8);
            DrawThinSeparator();
            EditorGUILayout.Space(4);
            GUILayout.Label("🎥 FOCUSCAM IN-GAME TESTING", EditorStyles.boldLabel);
            
            if (_debugFocusTimeline != null)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_debugFocusTimeline, new GUIContent("Focus Timeline"));
                EditorGUILayout.PropertyField(_debugBlendIn, new GUIContent("Blend In (s)"));
                EditorGUILayout.PropertyField(_debugBlendOut, new GUIContent("Blend Out (s)"));
                
                EditorGUILayout.Space(4);
                GUI.backgroundColor = OkColor;
                if (GUILayout.Button("🎬 Play Test Focus In-Game", GUILayout.Height(28)))
                {
                    if (Application.isPlaying && rig.Sequences != null)
                    {
                        _sequenceSerialized.ApplyModifiedProperties();
                        GameObject testObj = rig.Sequences.EditorTestFocusTimeline();
                        
                        if (testObj != null)
                        {
                            Selection.activeGameObject = testObj;
                            EditorApplication.ExecuteMenuItem("Window/Sequencing/Timeline");
                        }
                    }
                    else
                    {
                        Debug.LogWarning("[CameraTool] Test Focus only works in Play Mode.");
                    }
                }
                GUI.backgroundColor = Color.white;
                EditorGUI.indentLevel--;
            }
        }

        private void DrawSuiteHeader()
        {
            var rect = EditorGUILayout.GetControlRect(false, 40);
            EditorGUI.DrawRect(rect, HeaderBg);
            EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - 2, rect.width, 2), CyanAccent);
            GUI.Label(new Rect(rect.x + 12, rect.y + 8, rect.width, 24), "MTRMA  |  CAMERA SUITE PRO", _headerStyle);
        }

        private static bool DrawSectionHeader(string title, bool foldout)
        {
            EditorGUILayout.Space(12);
            var rect = EditorGUILayout.GetControlRect(false, 24);
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, 1), LineColor);
            EditorGUI.DrawRect(new Rect(rect.x, rect.y + 6, 2, 12), CyanAccent);
            var style = new GUIStyle(EditorStyles.foldoutHeader) { fontSize = 12, fontStyle = FontStyle.Bold };
            return EditorGUI.Foldout(new Rect(rect.x + 10, rect.y + 4, rect.width - 10, 20), foldout, title.ToUpper(), true, style);
        }

        private static void DrawThinSeparator()
        {
            var rect = EditorGUILayout.GetControlRect(false, 1);
            EditorGUI.DrawRect(rect, LineColor);
        }

        private void InitStyles()
        {
            if (_headerStyle != null) return;
            _headerStyle = new GUIStyle(EditorStyles.label) { fontSize = 18, fontStyle = FontStyle.Bold, normal = { textColor = Color.white } };
            _statusOkStyle = new GUIStyle(EditorStyles.miniLabel) { fontSize = 10, fontStyle = FontStyle.Bold, normal = { textColor = OkColor } };
            _statusBadStyle = new GUIStyle(EditorStyles.miniLabel) { fontSize = 10, fontStyle = FontStyle.Bold, normal = { textColor = BadColor } };
        }

        #endregion

        #region --- Logic Hub ---

        private void GenerateTimelineClips(CameraRig rig, int index)
        {
            var chapter = rig.Sequences.Chapters[index]; 
            TimelineAsset timeline = chapter.timeline;
            if (!timeline) 
            {
                Debug.LogWarning("[CameraTool] Chapter has no Timeline assigned!");
                return;
            }

            // 1. Find or Create Track (Reuse existing object to avoid Editor UI crashes)
            CameraToolTrack track = null;
            var tracks = timeline.GetOutputTracks();
            foreach (var t in tracks) 
            { 
               if (t is CameraToolTrack ct) { track = ct; break; } 
            }
            
            if (!track)
            {
                Undo.RegisterCompleteObjectUndo(timeline, "Create New Camera Track");
                track = timeline.CreateTrack<CameraToolTrack>(null, "Camera Sequence Track");
            }
            else
            {
                Undo.RegisterCompleteObjectUndo(track, "Clear Existing Clips");
                var clips = new System.Collections.Generic.List<TimelineClip>(track.GetClips());
                foreach (var c in clips) track.DeleteClip(c);
            }
            
            // 2. Preparation
            var segs = chapter.segments; 
            float totalMoveDur = 0f; 
            foreach (var s in segs) totalMoveDur += s.duration;
            
            int startRailIdx = chapter.startRailIndex; 
            int railLimit = Mathf.Min(startRailIdx + chapter.railCount, rig.Rails.RailCount);
            
            double nominalTimelineCursor = 0; 
            float accumulatedMoveDur = 0f;
            int clipsCreatedCount = 0;

            // 3. Generate Clips
            for (int r = startRailIdx; r < railLimit; r++) 
            {
                if (r >= rig.Rails.EditorRails.Count) break;
                var rail = rig.Rails.EditorRails[r]; 
                if (!rail) continue;

                int segmentsInThisRail = Mathf.Max(0, rail.pointCount - 1);
                if (segmentsInThisRail <= 0) continue;

                int currentRailFirstSegmentIdx = 0; 
                for (int p = startRailIdx; p < r; p++) 
                {
                    if (p < rig.Rails.EditorRails.Count && rig.Rails.EditorRails[p] != null)
                        currentRailFirstSegmentIdx += Mathf.Max(0, rig.Rails.EditorRails[p].pointCount - 1);
                }

                float railMoveDuration = 0;
                for (int s = 0; s < segmentsInThisRail; s++) 
                {
                    int totalSegIdx = currentRailFirstSegmentIdx + s;
                    if (totalSegIdx < segs.Count)
                        railMoveDuration += segs[totalSegIdx].duration;
                }

                float prevJunctionDur = 0;
                if (clipsCreatedCount > 0)
                {
                    prevJunctionDur = JunctionSettings.Default.duration; 
                    int prevRailLastSegmentIdx = currentRailFirstSegmentIdx - 1;
                    if (prevRailLastSegmentIdx >= 0 && prevRailLastSegmentIdx < segs.Count)
                    {
                        var overrideDur = segs[prevRailLastSegmentIdx].junctionOverride.duration;
                        if (overrideDur > 0.001f) prevJunctionDur = overrideDur;
                    }
                }

                float nextJunctionDur = 0;
                int currentRailLastSegmentIdx = currentRailFirstSegmentIdx + segmentsInThisRail - 1;
                if (r < railLimit - 1 && currentRailLastSegmentIdx < segs.Count)
                {
                    nextJunctionDur = JunctionSettings.Default.duration;
                    var overrideDur = segs[currentRailLastSegmentIdx].junctionOverride.duration;
                    if (overrideDur > 0.001f) nextJunctionDur = overrideDur;
                }

                float waitPadding = (clipsCreatedCount == 0) ? chapter.waitAtStart : 0f;

                // --- ROBUST CREATION (On Fresh Track) ---
                TimelineClip clip = track.CreateClip<CameraToolClip>();
                clip.displayName = $"Seq #{r:00} ({rail.name})"; 
                
                // Safety: Ensure the asset is actually rooted in the Timeline file
                if (clip.asset != null && !AssetDatabase.Contains(clip.asset))
                {
                    AssetDatabase.AddObjectToAsset(clip.asset, timeline);
                }
                
                clip.start = nominalTimelineCursor - (double)prevJunctionDur;
                if (clip.start < 0) clip.start = 0;
                clip.duration = (double)railMoveDuration + (double)prevJunctionDur + (double)nextJunctionDur + (double)waitPadding;

                clip.blendInDuration = (double)prevJunctionDur;
                clip.blendOutDuration = (double)nextJunctionDur;

                clipsCreatedCount++;
                
                var asset = clip.asset as CameraToolClip;
                if (asset != null)
                {
                    asset.Template.railIndex = r;
                    asset.Template.chapterIndex = rig.Sequences.Chapters.IndexOf(chapter);
                    asset.Template.clipStartTime = clip.start;
                    asset.Template.clipDuration = clip.duration;
                    asset.Template.startPadding = prevJunctionDur + waitPadding;
                    asset.Template.endPadding = nextJunctionDur;
                    asset.Template.startProgress = accumulatedMoveDur / totalMoveDur;
                    asset.Template.endProgress = (accumulatedMoveDur + railMoveDuration) / totalMoveDur;
                    
                    EditorUtility.SetDirty(asset);
                }

                nominalTimelineCursor += (double)railMoveDuration + (double)nextJunctionDur + (double)waitPadding;
                accumulatedMoveDur += railMoveDuration;
            }

            // 4. Persistence & Sync
            if (rig.Sequences.Director) 
            {
                rig.Sequences.Director.playableAsset = timeline;
                rig.Sequences.Director.SetGenericBinding(track, rig);
                EditorUtility.SetDirty(rig.Sequences.Director);
            }

            #if UNITY_EDITOR
            // 5. THE CRITICAL SYNC: Save to disk FIRST, then import/refresh
            EditorUtility.SetDirty(timeline);
            EditorUtility.SetDirty(track);
            AssetDatabase.SaveAssets(); 

            string assetPath = AssetDatabase.GetAssetPath(timeline);
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            
            UnityEditor.Timeline.TimelineEditor.Refresh(UnityEditor.Timeline.RefreshReason.ContentsAddedOrRemoved);
            #endif
            
            Debug.Log($"<color=#1ebfff><b>[CameraTool]</b></color> SUCCESS: Created {clipsCreatedCount} clips on Timeline '{timeline.name}'");
        }

        private void StartPreview(CameraRig rig)
        {
            var chapter = rig.Sequences.Chapters[_selectedChapterIndex.intValue]; var segs = chapter.segments; if (segs.Count == 0) return;
            _previewSegStarts = new float[segs.Count]; _previewSegDurations = new float[segs.Count]; _previewSegCurves = new AnimationCurve[segs.Count];
            float time = 0; for (int i = 0; i < segs.Count; i++) {
                _previewSegStarts[i] = time; _previewSegDurations[i] = segs[i].duration; _previewSegCurves[i] = segs[i].easing;
                time += _previewSegDurations[i] + segs[i].waitAtEnd;
            }
            _previewTotalDuration = time; _previewStartTime = EditorApplication.timeSinceStartup; _isPreviewing = true; EditorApplication.update += PreviewUpdate;
        }

        private void StopPreview() { if (!_isPreviewing) return; _isPreviewing = false; EditorApplication.update -= PreviewUpdate; }

        private void PreviewUpdate()
        {
            if (!_isPreviewing || target == null) { StopPreview(); return; }
            float elapsed = (float)(EditorApplication.timeSinceStartup - _previewStartTime);
            if (elapsed >= _previewTotalDuration) { ((CameraRig)target).EditorReportVisualState(-1, -1, 1f); StopPreview(); return; }
            int count = _previewSegStarts.Length; float p = 0;
            for (int i = 0; i < count; i++) {
                if (elapsed < _previewSegStarts[i] + _previewSegDurations[i]) {
                    float t = _previewSegCurves[i].Evaluate((elapsed - _previewSegStarts[i]) / _previewSegDurations[i]);
                    p = Mathf.Lerp((float)i / count, (float)(i + 1) / count, t); break;
                }
            }
            ((CameraRig)target).EditorReportVisualState(-1, -1, p); SceneView.RepaintAll();
        }

        private void SyncSceneView(SceneView sv)
        {
            if (!_isCameraLocked || Application.isPlaying) return;
            var cam = ((CameraRig)target).TargetCamera; if (cam == null) return;
            sv.pivot = cam.transform.position; sv.rotation = cam.transform.rotation; sv.size = 0; sv.Repaint();
        }

        #endregion
    }
}
