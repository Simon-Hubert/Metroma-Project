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
            clip.displayName = " "; // Use a space to avoid Unity showing "(Empty)"
        }

        public override void OnClipChanged(TimelineClip clip)
        {
            if (string.IsNullOrEmpty(clip.displayName) || clip.displayName == "(Empty)")
                clip.displayName = " ";
        }

        public override void DrawBackground(TimelineClip clip, ClipBackgroundRegion region)
        {
            base.DrawBackground(clip, region);

            FocusCamClip asset = clip.asset as FocusCamClip;
            if (asset == null) return;

            Rect rect = region.position;
            if (rect.width < 10) return;

            // --- 1. Blend & Safe Zone ---
            float blendInWidth = (float)(clip.blendInDuration / clip.duration) * rect.width;
            float blendOutWidth = (float)(clip.blendOutDuration / clip.duration) * rect.width;
            
            float safeX = rect.x + blendInWidth;
            float safeWidth = rect.width - blendInWidth - blendOutWidth;

            // If we have almost no safe space, we fallback to the visible rect
            if (safeWidth < 30)
            {
                safeX = rect.x;
                safeWidth = rect.width;
            }

            // --- 2. Styles ---
            GUIStyle labelStyle = new GUIStyle(EditorStyles.miniLabel);
            labelStyle.normal.textColor = Color.white;
            labelStyle.fontStyle = FontStyle.Bold;

            GUIStyle valueStyle = new GUIStyle(EditorStyles.miniLabel);
            valueStyle.normal.textColor = new Color(1, 1, 1, 0.6f);
            valueStyle.fontSize = 9;

            // --- 3. Full-Width Header Bar ---
            Rect headerRect = new Rect(safeX, rect.y + 3, safeWidth, 16);
            
            // Draw a solid dark band that spans the safe width
            EditorGUI.DrawRect(headerRect, new Color(0, 0, 0, 0.5f));

            string modeName = asset.mode == FocusMode.LookAtPoint ? "LookAt" : "Fixed";
            string icon = asset.mode == FocusMode.LookAtPoint ? "🎯" : "🧭";
            GUI.Label(new Rect(headerRect.x + 4, headerRect.y, headerRect.width - 8, headerRect.height), $"{icon} {modeName}", labelStyle);

            // --- 4. Values Line ---
            if (rect.height > 28 && safeWidth > 40)
            {
                string coords = asset.mode == FocusMode.LookAtPoint 
                    ? $"{asset.position.x:F0}, {asset.position.y:F0}, {asset.position.z:F0}" 
                    : $"{asset.rotation.x:F0}, {asset.rotation.y:F0}, {asset.rotation.z:F0}";

                Rect valueRect = new Rect(safeX + 6, rect.y + 17, safeWidth - 10, 14);
                GUI.Label(valueRect, coords, valueStyle);

                if (rect.height > 42)
                {
                    string extras = (asset.overridePosition ? "🎥 " : "") + (asset.overrideFOV ? "🔎" : "");
                    if (!string.IsNullOrEmpty(extras))
                    {
                        Rect extraRect = new Rect(safeX + 6, rect.y + 29, safeWidth - 10, 14);
                        GUI.Label(extraRect, extras, labelStyle);
                    }
                }
            }
        }
    }
}
