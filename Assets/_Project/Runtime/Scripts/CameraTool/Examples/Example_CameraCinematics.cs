using UnityEngine;
using Metroma.CameraTool;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using NaughtyAttributes;

namespace Metroma.CameraTool.Examples
{
    /// <summary>
    /// Exemple montrant comment lancer des chapitres sur la timeline principale (Rail)
    /// ou lancer des Focus Cam indépendantes (Standalones).
    /// </summary>
    public class Example_CameraCinematics : MonoBehaviour
    {
        [Header("Chapitre")]
        [SerializeField] private int chapterIndexToTest = 0;
        
        [Header("Focus Cam (Standalone)")]
        [SerializeField] private PlayableDirector focusDirector;
        [SerializeField] private TimelineAsset focusTimeline;

        [Button("🎬 Lancer un Chapitre (Rail Principal)")]
        public void PlayMainChapter()
        {
            if (CameraRig.Active == null || CameraRig.Active.Sequences == null)
            {
                Debug.LogWarning("Aucun CameraRig actif ou module Sequences trouvé.");
                return;
            }
            
            // Lance un chapitre de la Timeline Principale (Rail)
            CameraRig.Active.Sequences.PlayChapter(chapterIndexToTest, 1.5f, 5f);
        }

        [Button("🎥 Lancer une Focus Cam (Cinématique Indépendante)")]
        public void PlayStandaloneFocus()
        {
            if (CameraRig.Active == null || CameraRig.Active.Sequences == null || focusDirector == null || focusTimeline == null)
            {
                Debug.LogWarning("Il manque des références ou le CameraRig n'est pas actif.");
                return;
            }

            // Joue une Timeline de Focus de façon indépendante, en dehors du Rail principal.
            CameraRig.Active.Sequences.PlayFocusStandalone(
                InDirector: focusDirector, 
                InTimeline: focusTimeline, 
                InBlendIn: 1f,
                InBlendOut: 1f,
                InReturnToLastPos: false, // Si false, la caméra restera figée sur le dernier plan de la Focus Cam
                InOnStart: () => Debug.Log("Focus Cinématique Début !"), 
                InOnEnd: () => Debug.Log("Focus Cinématique Fin !")
            );
        }
    }
}
