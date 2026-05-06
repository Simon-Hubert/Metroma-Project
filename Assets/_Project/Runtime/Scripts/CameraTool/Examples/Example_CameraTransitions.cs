using UnityEngine;
using Metroma.CameraTool;
using NaughtyAttributes;

namespace Metroma.CameraTool.Examples
{
    /// <summary>
    /// Exemple illustrant comment détacher la caméra du rail pour aller vers
    /// une pose personnalisée dans l'espace, puis la faire revenir sur le rail.
    /// </summary>
    public class Example_CameraTransitions : MonoBehaviour
    {
        [Button("🛤️ Transition Libre vers une Pose Fixe")]
        public void TransitionToFreePose()
        {
            if (CameraRig.Active == null || CameraRig.Active.Transitions == null) return;

            // Créer une Pose manuelle (position / rotation / fov dans l'espace)
            CameraPose targetPose = new CameraPose
            {
                position = CameraRig.Active.CameraTransform.position + Vector3.up * 5f, // 5 mètres plus haut
                rotation = Quaternion.Euler(45f, 0f, 0f), // Regarde un peu vers le bas
                fov = 60f,
                up = Vector3.up
            };

            // Ordonne à la caméra de quitter le rail pour se rendre à cette position en 3 secondes
            CameraRig.Active.Transitions.StartTransition(targetPose, InDuration: 3f);
        }

        [Button("🛤️ Retour au Rail (Return To Rail)")]
        public void ReturnToRail()
        {
            if (CameraRig.Active == null || CameraRig.Active.Transitions == null) return;
            
            // Une fois qu'une pose libre (ou un Focus) est terminée, cette fonction
            // redonne le contrôle à l'animation de la timeline de base en douceur.
            CameraRig.Active.Transitions.ReturnToRail(InSmoothness: 5f);
        }
    }
}
