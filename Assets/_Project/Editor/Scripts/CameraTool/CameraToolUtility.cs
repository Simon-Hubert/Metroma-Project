using UnityEngine;
using UnityEditor;
using Dreamteck.Splines;
using System.Collections.Generic;
using Metroma.CameraTool.Modules;

namespace Metroma.CameraTool.Editor
{
    /// <summary>
    /// Static utility class for Camera Tool editor operations.
    /// Extracts complex logic from the main Editor class to improve maintainability.
    /// </summary>
    public static class CameraToolUtility
    {
        /// <summary>
        /// Automatically scans the project for Timeline assets containing 'Camera' in their name
        /// and adds them as chapters to the rig.
        /// </summary>
        public static void AutoScanChapters(CameraRig InRig)
        {
            Debug.Log($"<color=#1ebfff><b>[CameraTool]</b></color> SCAN BUTTON CLICKED for {InRig.name}...");
            string[] guids = AssetDatabase.FindAssets("t:TimelineAsset");
            var sequenceModule = InRig.Sequences;
            
            if (sequenceModule == null) return;

            sequenceModule.EditorClearChapters();

            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Timeline.TimelineAsset>(path);
                
                if (asset != null && asset.name.IndexOf("camera", System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    var chapter = new CameraChapter
                    {
                        name = asset.name.Replace("Camera_", "").Replace("_Timeline", "").Replace("camera_", ""),
                        timeline = asset,
                        isExpanded = false
                    };
                    
                    sequenceModule.EditorAddChapter(chapter);
                }
            }
            
            Debug.Log($"<color=#1ebfff><b>[CameraTool]</b></color> AutoScan: Found {guids.Length} total Timeline assets. Matches: {(sequenceModule.Chapters != null ? sequenceModule.Chapters.Count : 0)}.");
            EditorUtility.SetDirty(sequenceModule);
        }

        /// <summary>
        /// Synchronizes the pacing segments for a chapter based on the physical points of the spline rails.
        /// </summary>
        public static void SyncChapterSegments(CameraRig InRig, int InChapterIndex)
        {
            var railModule = InRig.Rails;
            var sequenceModule = InRig.Sequences;

            if (railModule == null || sequenceModule == null || InChapterIndex < 0 || InChapterIndex >= sequenceModule.Chapters.Count)
            {
                return;
            }

            var chapter = sequenceModule.Chapters[InChapterIndex];
            var rails = railModule.EditorRails;
            
            int totalRequiredSegments = 0;
            int endIdx = Mathf.Min(chapter.startRailIndex + chapter.railCount, rails.Count);

            for (int i = chapter.startRailIndex; i < endIdx; i++)
            {
                if (rails[i] != null)
                {
                    totalRequiredSegments += Mathf.Max(0, rails[i].pointCount - 1);
                }
            }

            // Adjustment loop
            while (chapter.segments.Count > totalRequiredSegments)
            {
                chapter.segments.RemoveAt(chapter.segments.Count - 1);
            }
            
            while (chapter.segments.Count < totalRequiredSegments)
            {
                chapter.segments.Add(new CameraSplineSegment
                {
                    duration = 1f, 
                    easing = AnimationCurve.EaseInOut(0, 0, 1, 1)
                });
            }

            // Labeling
            int segmentIdx = 0;
            for (int r = chapter.startRailIndex; r < endIdx; r++)
            {
                if (rails[r] == null)
                {
                    continue;
                }
                
                string prefix = rails.Count > 1 ? $"R{r} " : "";
                for (int n = 0; n < rails[r].pointCount - 1; n++) 
                {
                    if (segmentIdx < chapter.segments.Count)
                    {
                        chapter.segments[segmentIdx++].label = $"{prefix}Node {n} → {n + 1}";
                    }
                }
            }
            
            EditorUtility.SetDirty(sequenceModule);
        }

        /// <summary>
        /// Automatically calculates the rail count for each chapter based on their start rail indices.
        /// </summary>
        public static void AutoCalculateRailCounts(CameraRig InRig)
        {
            var railModule = InRig.Rails;
            var sequenceModule = InRig.Sequences;

            if (railModule == null || sequenceModule == null || sequenceModule.Chapters.Count == 0)
            {
                return;
            }

            var chapters = sequenceModule.Chapters;
            var rails = railModule.EditorRails;

            var sortedChapters = new List<CameraChapter>(chapters);
            sortedChapters.Sort((a, b) => a.startRailIndex.CompareTo(b.startRailIndex));

            for (int i = 0; i < sortedChapters.Count; i++)
            {
                var current = sortedChapters[i];
                
                if (i < sortedChapters.Count - 1)
                {
                    var next = sortedChapters[i + 1];
                    current.railCount = Mathf.Max(1, next.startRailIndex - current.startRailIndex);
                }
                else
                {
                    current.railCount = Mathf.Max(1, rails.Count - current.startRailIndex);
                }
            }
            
            EditorUtility.SetDirty(sequenceModule);
        }

        /// <summary>
        /// Ensures all necessary modular components are added and initialized.
        /// </summary>
        public static void FullSetupRig(CameraRig InRig)
        {
            Undo.RecordObject(InRig.gameObject, "Setup Camera Rig");

            // 1. Add Modules
            if (!InRig.GetComponent<RailModule>()) Undo.AddComponent<RailModule>(InRig.gameObject);
            if (!InRig.GetComponent<SequenceModule>()) Undo.AddComponent<SequenceModule>(InRig.gameObject);
            if (!InRig.GetComponent<TransitionModule>()) Undo.AddComponent<TransitionModule>(InRig.gameObject);

            // 2. Add Timeline Support
            if (!InRig.GetComponent<UnityEngine.Playables.PlayableDirector>()) 
                Undo.AddComponent<UnityEngine.Playables.PlayableDirector>(InRig.gameObject);

            // 3. Auto-find Camera
            if (InRig.TargetCamera == null)
            {
                var mainCam = UnityEngine.Camera.main;
                if (mainCam != null)
                {
                    SerializedObject so = new SerializedObject(InRig);
                    so.FindProperty("targetCamera").objectReferenceValue = mainCam;
                    so.ApplyModifiedProperties();
                }
            }

            Debug.Log($"<color=#1ebfff><b>[CameraTool]</b></color> Rig Setup complete for {InRig.name}.");
            EditorUtility.SetDirty(InRig.gameObject);
        }

        /// <summary>
        /// Calculates the physical length of a series of rails.
        public static float CalculatePhysicalLength(List<SplineComputer> InRails, int InStart, int InCount)
        {
            float total = 0;
            int end = Mathf.Min(InStart + InCount, InRails.Count);
            
            for (int i = InStart; i < end; i++)
            {
                if (InRails[i] != null)
                {
                    total += InRails[i].CalculateLength();
                }
            }
            return total;
        }
    }
}
