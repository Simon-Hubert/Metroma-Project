using System;
using UnityEngine;
using UnityEngine.UI;

namespace Metroma.UI
{
    public class MainMenuPanel : UIPanel
    {
        [Header("Menu Buttons")]
        [SerializeField] private Button StartButton;
        [SerializeField] private Button OptionButton;
        [SerializeField] private Button QuitButton;
        
        public static event Action OnStartGameRequested;
        public static event Action OnQuitGameRequested;

        public override void Initialize()
        {
            base.Initialize();

            if (StartButton != null)
                StartButton.onClick.AddListener(OnStartClicked);
                
            if (OptionButton != null)
                OptionButton.onClick.AddListener(OnOptionClicked);
                
            if (QuitButton != null)
                QuitButton.onClick.AddListener(OnQuitClicked);
        }

        private void OnDestroy()
        {
            if (StartButton != null)
                StartButton.onClick.RemoveListener(OnStartClicked);
                
            if (OptionButton != null)
                OptionButton.onClick.RemoveListener(OnOptionClicked);
                
            if (QuitButton != null)
                QuitButton.onClick.RemoveListener(OnQuitClicked);
        }

        private void OnStartClicked()
        {
            Debug.Log("[MainMenuPanel] Start Game Clicked!");
            OnStartGameRequested?.Invoke();
        }

        private void OnOptionClicked()
        {
            Debug.Log("[MainMenuPanel] Options Clicked!");
            UIManager.Instance.OpenPanel<Panels.OptionsPanel>();
        }

        private void OnQuitClicked()
        {
            Debug.Log("[MainMenuPanel] Quit Clicked! Opening PowerOff Popup...");
            UIManager.Instance.OpenPanel<Panels.PowerOffPanel>();
        }
    }
}
