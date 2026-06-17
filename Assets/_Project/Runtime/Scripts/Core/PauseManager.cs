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
        [SerializeField]
        private string MainMenuSceneName = "MainMenu_Test";

        [Header("Additional Pause UI")]
        [SerializeField]
        private System.Collections.Generic.List<GameObject> AdditionalPauseUIElements = new System.Collections.Generic.List<GameObject>();

        [SerializeField]
        private float FadeDuration = 0.25f;

        [Header("Animations")]
        [SerializeField]
        private Metroma.UI.Effects.JuicyEntrance PauseEntranceAnimation;

        public static bool IsPaused { get; private set; } = false;

        private Coroutine _fadeCoroutine;

        private void Start()
        {
            foreach (var element in AdditionalPauseUIElements)
            {
                if (element != null)
                {
                    var group = element.GetComponent<CanvasGroup>();
                    if (group == null) group = element.AddComponent<CanvasGroup>();
                    
                    group.alpha = 0f;
                    group.blocksRaycasts = false;
                    group.interactable = false;
                }
            }
        }

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
            
            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = StartCoroutine(FadeAdditionalElements(1f, true));

            if (PauseEntranceAnimation != null)
            {
                PauseEntranceAnimation.Play();
            }
        }

        private void OnMenuClosed()
        {
            IsPaused = false;
            Time.timeScale = 1f;
            
            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = StartCoroutine(FadeAdditionalElements(0f, false));
        }

        private System.Collections.IEnumerator FadeAdditionalElements(float targetAlpha, bool bBlocksRaycasts)
        {
            var groups = new System.Collections.Generic.List<CanvasGroup>();
            foreach (var element in AdditionalPauseUIElements)
            {
                if (element != null)
                {
                    var g = element.GetComponent<CanvasGroup>();
                    if (g != null) groups.Add(g);
                }
            }

            if (groups.Count == 0) yield break;

            float startAlpha = groups[0].alpha;
            float elapsed = 0f;

            while (elapsed < FadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float currentAlpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / FadeDuration);
                
                foreach (var g in groups)
                {
                    g.alpha = currentAlpha;
                }
                
                yield return null;
            }

            foreach (var g in groups)
            {
                g.alpha = targetAlpha;
                g.blocksRaycasts = bBlocksRaycasts;
                g.interactable = bBlocksRaycasts;
            }
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
