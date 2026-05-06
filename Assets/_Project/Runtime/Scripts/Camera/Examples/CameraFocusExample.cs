using UnityEngine;
using Metroma.CameraTool;
using Metroma.CameraTool.Modules;
using UnityEngine.Timeline;
using UnityEngine.Playables;
using NaughtyAttributes;

namespace Metroma.CameraTool.Examples
{
    /// <summary>
    /// Exemple montrant comment lancer des séquences de FocusCam (Cinématiques).
    /// FocusCam est le système central pour orchestrer les sous-timelines.
    /// </summary>
    public class CameraFocusExample : MonoBehaviour
    {
        [Header("Références")]
        public CameraRig rig;
        public PlayableDirector focusDirector;
        public TimelineAsset focusTimeline;
        
        [Header("Settings")]
        public float blendIn = 1.0f;
        public float blendOut = 1.0f;

        [Button("🎥 Lancer Focus Cam (Code)")]
        public void TriggerFocus()
        {
            // Get the sequence module
            var sequenceModule = rig.Sequences;

            if (sequenceModule == null)
            {
                Debug.LogWarning("SequenceModule introuvable sur le Rig.");
                return;
            }

            // Joue une Timeline de Focus indépendante.
            // Le système gère le mélange, le pause/play de la timeline principale et le retour.
            sequenceModule.PlayFocusStandalone(
                focusDirector != null ? focusDirector : sequenceModule.Director, 
                focusTimeline, 
                blendIn, 
                blendOut, 
                true, // returnToLastPos: Revient exactement d'où on venait
                () => Debug.Log("Focus : Début de la séquence"), 
                () => Debug.Log("Focus : Fin de la séquence")
            );
        }

        // --- Écoute des événements via Code ---
        
        private void OnEnable()
        {
            if (rig != null && rig.Sequences != null)
            {
                rig.Sequences.OnFocusStarted += OnAnyFocusStarted;
                rig.Sequences.OnFocusEnded += OnAnyFocusEnded;
            }
        }

        private void OnDisable()
        {
            if (rig != null && rig.Sequences != null)
            {
                rig.Sequences.OnFocusStarted -= OnAnyFocusStarted;
                rig.Sequences.OnFocusEnded -= OnAnyFocusEnded;
            }
        }

        private void OnAnyFocusStarted() => Debug.Log("Un focus vient de commencer !");
        private void OnAnyFocusEnded() => Debug.Log("Un focus vient de se terminer !");
    }
}
