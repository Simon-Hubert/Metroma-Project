using UnityEngine;
using Metroma.CameraTool;
using Metroma.CameraTool.Modules;
using Metroma.CameraTool.Modifiers;
using NaughtyAttributes;

namespace Metroma.CameraTool.Examples
{
    /// <summary>
    /// Exemple démontrant l'utilisation du module FPS pour passer en mode
    /// observation libre (souris/manette) pendant un certain temps ou manuellement.
    /// </summary>
    public class Example_CameraFPSMode : MonoBehaviour
    {
        [SerializeField] private CameraFPSProfile fpsProfile;

        [Button("🔫 Activer Mode FPS (5 secondes)")]
        public void EnableFPSModeTemporarily()
        {
            if (CameraRig.Active == null || CameraRig.Active.FPS == null || fpsProfile == null)
            {
                Debug.LogWarning("Il manque le profile FPS ou le CameraRig n'est pas actif.");
                return;
            }

            // Fige la caméra à sa position actuelle et donne le contrôle de la rotation au joueur (souris/joystick)
            CameraRig.Active.FPS.EnableFPS(
                InPosition: CameraRig.Active.CameraTransform.position, 
                InRotation: CameraRig.Active.CameraTransform.rotation, 
                InProfile: fpsProfile, 
                InDuration: 5f,      // Durée (Mettre 0 pour que ce soit infini jusqu'à Disable)
                InExitDuration: 1f   // Temps mis pour retourner sur le rail à la fin
            );
        }

        [Button("🔫 Désactiver Mode FPS (Manuellement)")]
        public void DisableFPSModeManually()
        {
            if (CameraRig.Active == null || CameraRig.Active.FPS == null)
            {
                return;
            }
            
            CameraRig.Active.FPS.DisableFPS();
        }
    }
}
