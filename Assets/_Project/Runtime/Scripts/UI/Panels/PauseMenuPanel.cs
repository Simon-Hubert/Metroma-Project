using System;
using UnityEngine;
using UnityEngine.UI;


namespace Metroma.UI.Panels
{

    public class PauseMenuPanel : UIPanel
    {
        // --- Settings ---

        [Header("Pause Buttons")]
        [SerializeField, Tooltip("Bouton pour reprendre la partie.")]
        private Button ResumeButton;
        
        [SerializeField, Tooltip("Bouton pour ouvrir les paramètres.")]
        private Button OptionButton;
        
        [SerializeField, Tooltip("Bouton pour quitter vers le menu principal.")]
        private Button MainMenuButton;

        public static event Action OnReturnToMainMenuRequested;
        public static event Action OnPauseMenuOpened;
        public static event Action OnPauseMenuClosed;

        // --- Lifecycle ---

        public override void Show()
        {
            base.Show();
            OnPauseMenuOpened?.Invoke();
        }

        public override void Hide()
        {
            base.Hide();
            OnPauseMenuClosed?.Invoke();
        }

        public override void Initialize()
        {
            bIsPopup = true;
            
            base.Initialize();

            if (ResumeButton != null)
                ResumeButton.onClick.AddListener(OnResumeClicked);
                
            if (OptionButton != null)
                OptionButton.onClick.AddListener(OnOptionClicked);
                
            if (MainMenuButton != null)
                MainMenuButton.onClick.AddListener(OnMainMenuClicked);
        }


        private void OnDestroy()
        {
            if (ResumeButton != null)
                ResumeButton.onClick.RemoveListener(OnResumeClicked);
                
            if (OptionButton != null)
                OptionButton.onClick.RemoveListener(OnOptionClicked);
                
            if (MainMenuButton != null)
                MainMenuButton.onClick.RemoveListener(OnMainMenuClicked);
        }


        // --- Event Handlers ---

        private void OnResumeClicked()
        {
            Debug.Log("[PauseMenuPanel] Reprise du jeu !");
            UIManager.Instance.CloseCurrentPanel();
        }

        private void OnOptionClicked()
        {
            Debug.Log("[PauseMenuPanel] Ouverture des Options...");
            UIManager.Instance.OpenPanel<PauseOptionsPanel>();
        }

        private void OnMainMenuClicked()
        {
            Debug.Log("[PauseMenuPanel] Retour au Menu Principal demandé !");
            OnReturnToMainMenuRequested?.Invoke();
        }
    }
}
