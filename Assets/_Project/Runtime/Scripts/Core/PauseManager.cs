using UnityEngine;
using UnityEngine.InputSystem;
using Metroma.UI;
using Metroma.UI.Panels;

namespace Metroma.Core
{
    /**
     * @brief Gère l'entrée joueur pour afficher ou cacher le menu Pause.
     * Maintient son état synchronisé avec l'UI grâce aux événements.
     */
    public class PauseManager : MonoBehaviour
    {
        private bool bIsPaused = false;

        private void OnEnable()
        {
            // S'abonne aux événements qu'on vient de créer pour rester synchronisé 
            // au cas où le joueur clique sur le bouton "Reprendre" à la souris.
            PauseMenuPanel.OnPauseMenuOpened += OnMenuOpened;
            PauseMenuPanel.OnPauseMenuClosed += OnMenuClosed;
        }

        private void OnDisable()
        {
            PauseMenuPanel.OnPauseMenuOpened -= OnMenuOpened;
            PauseMenuPanel.OnPauseMenuClosed -= OnMenuClosed;
        }

        private void OnMenuOpened() => bIsPaused = true;
        private void OnMenuClosed() => bIsPaused = false;


        private void Update()
        {
            // Vérifie si "Echap" (Clavier) ou le bouton "Start/Options" (Manette) a été pressé
            bool bPausePressed = false;

            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                bPausePressed = true;
            
            if (Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame)
                bPausePressed = true;

            // Si le joueur veut mettre en pause / reprendre
            if (bPausePressed)
            {
                TogglePauseUI();
            }
        }

        private void TogglePauseUI()
        {
            if (!bIsPaused)
            {
                // On n'est pas en pause, on ouvre le menu
                UIManager.Instance.OpenPanel<PauseMenuPanel>();
            }
            else
            {
                // On est déjà en pause, on demande à l'UI de fermer le panneau actuel
                UIManager.Instance.CloseCurrentPanel();
            }
        }
    }
}
