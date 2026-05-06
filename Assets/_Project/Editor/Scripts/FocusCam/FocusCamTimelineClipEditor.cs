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
        private readonly Color _metromaPink = new Color(0.95f, 0.2f, 0.45f);
        private readonly Color _metromaBlue = new Color(0.15f, 0.6f, 1.0f);
        private readonly Color _metromaDark = new Color(0.12f, 0.12f, 0.15f, 0.9f);

        public override ClipDrawOptions GetClipOptions(TimelineClip clip)
        {
            ClipDrawOptions options = base.GetClipOptions(clip);
            
            FocusCamClip asset = clip.asset as FocusCamClip;
            if (asset != null)
            {
                options.tooltip = asset.mode == FocusMode.LookAtPoint ? "🎯 Focus Point" : "🧭 Fixed Orientation";
                Color hColor = asset.mode == FocusMode.LookAtPoint ? _metromaPink : _metromaBlue;
                if (asset.useCustomColor) hColor = asset.customColor;
                options.highlightColor = hColor;
            }

            // Supprime le nom par défaut "Focus Cam Clip Asset" d'Unity
            if (clip.displayName != " ") clip.displayName = " ";

            return options;
        }

        public override void DrawBackground(TimelineClip clip, ClipBackgroundRegion region)
        {
            FocusCamClip asset = clip.asset as FocusCamClip;
            if (asset == null) return;

            Rect rect = region.position;
            if (rect.width < 5) return;

            // --- 1. FOND SOMBRE ---
            EditorGUI.DrawRect(rect, _metromaDark);

            // --- 2. DESSIN DES TRANSITIONS (BLEU/ROSE) ---
            float blendInPx = clip.duration > 0 ? (float)(clip.blendInDuration / clip.duration) * rect.width : 0;
            float blendOutPx = clip.duration > 0 ? (float)(clip.blendOutDuration / clip.duration) * rect.width : 0;
            Color modeColor = asset.mode == FocusMode.LookAtPoint ? _metromaPink : _metromaBlue;
            if (asset.useCustomColor) modeColor = asset.customColor;

            if (blendInPx > 1)
            {
                EditorGUI.DrawRect(new Rect(rect.x, rect.y, blendInPx, rect.height), new Color(modeColor.r, modeColor.g, modeColor.b, 0.2f));
                EditorGUI.DrawRect(new Rect(rect.x, rect.y, 2, rect.height), modeColor);
            }

            if (blendOutPx > 1)
            {
                EditorGUI.DrawRect(new Rect(rect.xMax - blendOutPx, rect.y, blendOutPx, rect.height), new Color(modeColor.r, modeColor.g, modeColor.b, 0.2f));
                EditorGUI.DrawRect(new Rect(rect.xMax - 2, rect.y, 2, rect.height), modeColor);
            }

            // --- 3. TEXTE (PUNAISÉ AU BORD GAUCHE) ---
            // On fixe le texte avec une petite marge absolue de 8 pixels, il ne bougera PLUS JAMAIS.
            float availableWidth = rect.width - 16;

            if (availableWidth > 20)
            {
                // Styles stricts qui forcent Unity à couper au ciseau si le clip est trop petit
                GUIStyle labelStyle = new GUIStyle(EditorStyles.boldLabel) { 
                    fontSize = 10, normal = { textColor = Color.white }, 
                    alignment = TextAnchor.UpperLeft, clipping = TextClipping.Clip 
                };
                GUIStyle miniStyle = new GUIStyle(EditorStyles.miniLabel) { 
                    fontSize = 8, normal = { textColor = new Color(1, 1, 1, 0.5f) }, 
                    alignment = TextAnchor.UpperLeft, clipping = TextClipping.Clip 
                };

                string icon = asset.mode == FocusMode.LookAtPoint ? "🎯" : "🧭";
                string modeName = asset.mode == FocusMode.LookAtPoint ? "LOOK AT" : "FIXED";
                
                // Titre
                GUI.Label(new Rect(rect.x + 8, rect.y + 2, availableWidth, 16), $"{icon} {modeName}", labelStyle);
                
                // Sous-titre
                if (rect.height > 24)
                {
                    string subText = asset.mode == FocusMode.LookAtPoint ? "Position Target" : "Rotation Target";
                    if (asset.overrideFOV) subText += $" | 🔎 {asset.fov}°";
                    GUI.Label(new Rect(rect.x + 8, rect.y + 14, availableWidth, 12), subText, miniStyle);
                }
            }
            else if (rect.width > 20)
            {
                // Si le clip est très écrasé, on n'affiche que l'icône centrée
                GUIStyle iconStyle = new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleCenter, fontSize = 12, clipping = TextClipping.Clip };
                GUI.Label(new Rect(rect.x, rect.y, rect.width, rect.height), asset.mode == FocusMode.LookAtPoint ? "🎯" : "🧭", iconStyle);
            }

            // --- 4. BORDURE SUPÉRIEURE ---
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, 1), new Color(1, 1, 1, 0.1f));
        }

        public override void OnCreate(TimelineClip clip, TrackAsset track, TimelineClip clonedFrom)
        {
            clip.displayName = " ";
            clip.blendInDuration = 0.5f;
            clip.blendOutDuration = 0.5f;
        }
    }
}