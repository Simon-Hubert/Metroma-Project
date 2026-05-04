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

            // --- 1. Clipping Group ---
            // This ensures EVERYTHING drawn inside is clipped to the clip's rect.
            GUI.BeginGroup(rect);
            try
            {
                // --- 2. Local Coordinates (0,0 is now the top-left of the clip) ---
                float blendInPx = (float)(clip.blendInDuration / clip.duration) * rect.width;
                float safeOffset = Mathf.Min(blendInPx, rect.width * 0.5f);
                
                float localX = safeOffset + 5;
                // Clamp width so it NEVER exceeds the clip's right edge
                float labelWidth = Mathf.Min(135, rect.width - localX - 5);

                if (labelWidth > 10)
                {
                    // --- 3. Styles ---
                    GUIStyle labelStyle = new GUIStyle(EditorStyles.boldLabel);
                    labelStyle.normal.textColor = Color.white;
                    labelStyle.fontSize = 10;

                    GUIStyle valueStyle = new GUIStyle(EditorStyles.miniLabel);
                    valueStyle.normal.textColor = new Color(1, 1, 1, 0.7f);
                    valueStyle.fontSize = 9;

                    Color highlightColor = asset.mode == FocusMode.LookAtPoint 
                        ? new Color(0.95f, 0.2f, 0.45f, 0.8f) 
                        : new Color(0.15f, 0.6f, 1.0f, 0.8f);

                    // --- 4. Draw Header Bar ---
                    Rect headerRect = new Rect(localX, 2, labelWidth, 16);
                    EditorGUI.DrawRect(headerRect, new Color(0, 0, 0, 0.8f));
                    EditorGUI.DrawRect(new Rect(headerRect.x, headerRect.y, 3, headerRect.height), highlightColor);

                    string modeName = asset.mode == FocusMode.LookAtPoint ? "LookAt" : "Fixed";
                    string icon = asset.mode == FocusMode.LookAtPoint ? "🎯" : "🧭";
                    GUI.Label(new Rect(headerRect.x + 6, headerRect.y, headerRect.width - 8, headerRect.height), $"{icon} {modeName}", labelStyle);

                    // --- 5. Values Line ---
                    if (rect.height > 25)
                    {
                        string coords = asset.mode == FocusMode.LookAtPoint 
                            ? $"{asset.position.x:F0}, {asset.position.y:F0}, {asset.position.z:F0}" 
                            : $"{asset.rotation.x:F0}, {asset.rotation.y:F0}, {asset.rotation.z:F0}";

                        Rect valueRect = new Rect(localX + 2, 18, labelWidth, 14);
                        GUI.Label(valueRect, coords, valueStyle);
                    }
                }
            }
            finally
            {
                GUI.EndGroup();
            }
        }
    }
}
