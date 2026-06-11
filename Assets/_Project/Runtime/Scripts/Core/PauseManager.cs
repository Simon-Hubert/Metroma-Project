using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Metroma.UI;
using Metroma.UI.Panels;


namespace Metroma.Core
{

    public class PauseManager : MonoBehaviour
    {
        [Header("Scene Settings")]
        [SerializeField, Tooltip("Nom de la scène du menu principal à charger.")]
        private string MainMenuSceneName = "MainMenu_Test";

        public static bool IsPaused { get; private set; } = false;

        private void OnEnable()
        {
            PauseMenuPanel.OnPauseMenuOpened += OnMenuOpened;
            PauseMenuPanel.OnPauseMenuClosed += OnMenuClosed;
            PauseMenuPanel.OnReturnToMainMenuRequested += OnReturnToMainMenu;
        }

        private void OnDisable()
        {
            PauseMenuPanel.OnPauseMenuOpened -= OnMenuOpened;
            PauseMenuPanel.OnPauseMenuClosed -= OnMenuClosed;
            PauseMenuPanel.OnReturnToMainMenuRequested -= OnReturnToMainMenu;
        }

        private void OnReturnToMainMenu()
        {
            Time.timeScale = 1f;
            IsPaused = false;
            
            SceneManager.LoadScene(MainMenuSceneName);
        }

        private void OnMenuOpened()
        {
            IsPaused = true;
            Time.timeScale = 0f;
        }

        private void OnMenuClosed()
        {
            IsPaused = false;
            Time.timeScale = 1f;
        }


        private void Update()
        {
            bool bPausePressed = false;

            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                bPausePressed = true;
            
            if (Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame)
                bPausePressed = true;

            if (bPausePressed)
            {
                TogglePauseUI();
            }
        }

        private void TogglePauseUI()
        {
            if (!IsPaused)
            {
                UIManager.Instance.OpenPanel<PauseMenuPanel>();
            }
            else
            {
                UIManager.Instance.CloseCurrentPanel();
            }
        }
    }
}
