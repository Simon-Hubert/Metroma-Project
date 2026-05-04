using UnityEngine;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine.Timeline;
using Metroma.FocusCam;

namespace Metroma.FocusCam.Editor
{
    [CustomTimelineEditor(typeof(FocusCamClip))]
    public class FocusCamTimelineClipEditor : ClipEditor
    {
        public override ClipDrawOptions GetClipOptions(TimelineClip clip)
        {
            ClipDrawOptions options = base.GetClipOptions(clip);
            
            FocusCamClip asset = clip.asset as FocusCamClip;
            if (asset != null)
            {
                // Dynamic Title (Updated for tooltip)
                string title = asset.mode == FocusMode.LookAtPoint ? "🎯 Look At" : "🧭 Fixed Orientation";
                string details = "";
                if (asset.overridePosition) details += " | 🎥 Position Override";
                if (asset.overrideFOV) details += $" | 🔎 FOV: {asset.fov}";

                options.tooltip = title + details;
                
                // Colors matching the Metroma theme
                if (asset.mode == FocusMode.LookAtPoint)
                    options.highlightColor = new Color(0.95f, 0.2f, 0.45f); // Neon Pink
                else
                    options.highlightColor = new Color(0.15f, 0.6f, 1.0f); // Electric Blue
            }

            return options;
        }

        public override void OnCreate(TimelineClip clip, TrackAsset track, TimelineClip clonedFrom)
        {
            clip.displayName = ""; // We draw everything manually for a cleaner look
        }

        public override void OnClipChanged(TimelineClip clip)
        {
            clip.displayName = ""; // Keep it clean if edited
        }

        public override void DrawBackground(TimelineClip clip, ClipBackgroundRegion region)
        {
            base.DrawBackground(clip, region);

            FocusCamClip asset = clip.asset as FocusCamClip;
            if (asset == null) return;

            Rect rect = region.position;
            if (rect.width < 30) return; // Don't draw if too small

            // --- 1. Prepare Styles ---
            GUIStyle labelStyle = new GUIStyle(EditorStyles.miniLabel);
            labelStyle.normal.textColor = Color.white;
            labelStyle.fontStyle = FontStyle.Bold;
            labelStyle.alignment = TextAnchor.MiddleLeft;

            GUIStyle valueStyle = new GUIStyle(EditorStyles.miniLabel);
            valueStyle.normal.textColor = new Color(1, 1, 1, 0.6f);
            valueStyle.fontSize = 9;

            // --- 2. Calculate Content ---
            string icon = asset.mode == FocusMode.LookAtPoint ? "🎯" : "🧭";
            string modeName = asset.mode == FocusMode.LookAtPoint ? "LookAt" : "Fixed";
            string coords = asset.mode == FocusMode.LookAtPoint 
                ? $"({asset.position.x:F1}, {asset.position.y:F1}, {asset.position.z:F1})" 
                : $"({asset.rotation.x:F0}, {asset.rotation.y:F0}, {asset.rotation.z:F0})";

            // --- 3. Draw Header Label ---
            Rect headerRect = new Rect(rect.x + 4, rect.y + 4, rect.width - 8, 16);
            
            // Draw semi-transparent dark backing for readability
            EditorGUI.DrawRect(new Rect(headerRect.x - 2, headerRect.y, headerRect.width + 4, 14), new Color(0, 0, 0, 0.3f));
            GUI.Label(headerRect, $"{icon}  {modeName}", labelStyle);

            // --- 4. Draw Values Line ---
            if (rect.height > 30)
            {
                Rect valueRect = new Rect(rect.x + 6, rect.y + 18, rect.width - 12, 14);
                GUI.Label(valueRect, coords, valueStyle);

                // Add small sub-icons for overrides
                if (rect.height > 45)
                {
                    string extras = "";
                    if (asset.overridePosition) extras += " 🎥";
                    if (asset.overrideFOV) extras += " 🔎";
                    
                    if (!string.IsNullOrEmpty(extras))
                    {
                        Rect extraRect = new Rect(rect.x + 6, rect.y + 30, rect.width - 12, 14);
                        GUI.Label(extraRect, extras, labelStyle);
                    }
                }
            }
        }
    }
}
