using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Timeline;
using Metroma.CameraTool.Timeline;

namespace Metroma.CameraTool.Editor
{
    /// <summary>
    /// Customizes the visual representation of CameraToolClip on the Timeline.
    /// Highlights transition zones (paddings) and alternates colors for better readability.
    /// </summary>
    [CustomTimelineEditor(typeof(CameraToolClip))]
    public class CameraToolClipEditor : ClipEditor
    {
        private static readonly Color JunctionColor = new Color(1f, 0.45f, 0f, 0.4f); // Métrōma Orange (More Opaque)
        private static readonly Color DividerColor = new Color(1f, 0.45f, 0f, 0.8f);
        private static readonly Color RailColorEven = new Color(0.12f, 0.45f, 0.75f, 0.9f); // Deep Blue
        private static readonly Color RailColorOdd = new Color(0.15f, 0.6f, 0.6f, 0.9f);   // Teal

        public override ClipDrawOptions GetClipOptions(TimelineClip clip)
        {
            var options = base.GetClipOptions(clip);
            var asset = clip.asset as CameraToolClip;

            if (asset != null)
            {
                // Alternate colors based on rail index
                options.highlightColor = (asset.Template.railIndex % 2 == 0) ? RailColorEven : RailColorOdd;
                options.tooltip = $"Rail #{asset.Template.railIndex} | Chapter {asset.Template.chapterIndex}";
            }

            return options;
        }

        public override void DrawBackground(TimelineClip clip, ClipBackgroundRegion region)
        {
            var asset = clip.asset as CameraToolClip;
            if (asset == null) return;

            float duration = (float)clip.duration;
            if (duration <= 0.001f) return;

            // --- DRAW START PADDING (JUNCTION IN) ---
            if (asset.Template.startPadding > 0.001f)
            {
                float normalizedPadding = asset.Template.startPadding / duration;
                float pixelWidth = (float)region.position.width * normalizedPadding;

                Rect paddingRect = new Rect(region.position.x, region.position.y, pixelWidth, region.position.height);
                EditorGUI.DrawRect(paddingRect, JunctionColor);
                EditorGUI.DrawRect(new Rect(paddingRect.xMax - 1f, paddingRect.y, 1f, paddingRect.height), DividerColor);

                // Draw tiny "J" label
                if (pixelWidth > 20f)
                {
                    GUIStyle labelStyle = new GUIStyle(EditorStyles.miniLabel) { alignment = TextAnchor.MiddleCenter, normal = { textColor = Color.white } };
                    GUI.Label(new Rect(paddingRect.x, paddingRect.y, paddingRect.width, 15f), "J", labelStyle);
                }
            }

            // --- DRAW END PADDING (JUNCTION OUT) ---
            if (asset.Template.endPadding > 0.001f)
            {
                float normalizedPadding = asset.Template.endPadding / duration;
                float pixelWidth = (float)region.position.width * normalizedPadding;

                Rect paddingRect = new Rect(region.position.xMax - pixelWidth, region.position.y, pixelWidth, region.position.height);
                EditorGUI.DrawRect(paddingRect, JunctionColor);
                EditorGUI.DrawRect(new Rect(paddingRect.x, paddingRect.y, 1f, paddingRect.height), DividerColor);

                if (pixelWidth > 20f)
                {
                    GUIStyle labelStyle = new GUIStyle(EditorStyles.miniLabel) { alignment = TextAnchor.MiddleCenter, normal = { textColor = Color.white } };
                    GUI.Label(new Rect(paddingRect.x, paddingRect.y, paddingRect.width, 15f), "J", labelStyle);
                }
            }
        }
    }
}
