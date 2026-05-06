using UnityEngine;
using Metroma.CameraTool;
using Metroma.CameraTool.Modules;
using Metroma.CameraTool.Modifiers;
using NaughtyAttributes;

namespace Metroma.CameraTool.Examples
{
    /// <summary>
    /// Exemple illustrant l'activation du mode FPS (observation libre).
    /// Permet au joueur de regarder autour de lui avec la souris/joystick.
    /// </summary>
    public class CameraFPSExample : MonoBehaviour
    {
        [SerializeField] private CameraRig rig;
        [SerializeField] private CameraFPSProfile fpsProfile;

        [Button("🔫 Activer Mode FPS (Manuel)")]
        public void EnableFPS()
        {
            if (rig == null || rig.FPS == null || fpsProfile == null) return;

            // Active le mode FPS à partir de la position/rotation actuelle.
            // On peut définir une durée de sortie (exitDuration) pour le retour.
            rig.FPS.EnableFPS(
                rig.CameraTransform.position, 
                rig.CameraTransform.rotation, 
                fpsProfile, 
                InDuration: 0f,    // 0 = Infini jusqu'à appel manuel de Disable
                InExitDuration: 1.5f
            );
        }

        [Button("🔫 Désactiver Mode FPS")]
        public void DisableFPS()
        {
            if (rig != null && rig.FPS != null)
                rig.FPS.DisableFPS();
        }
    }
}
