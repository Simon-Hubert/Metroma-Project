using UnityEngine;
using UnityEngine.InputSystem;
using Metroma.UI;
using Metroma.UI.Panels;


namespace Metroma.Core
{

    public class PauseManager : MonoBehaviour
    {
        private bool bIsPaused = false;

        private void OnEnable()
        {
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
            if (!bIsPaused)
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
