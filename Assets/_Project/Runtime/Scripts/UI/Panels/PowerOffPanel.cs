using System.Collections;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Metroma.UI.Panels
{

    public class PowerOffPanel : UIPanel
    {
        // --- Settings ---

        [Header("Power Off Settings")]
        
        [SerializeField]
        private Button ConfirmButton;
        
        [SerializeField]
        private Button CancelButton;


        [Header("Animation Settings")]
        
        [SerializeField, Tooltip("Durée de l'animation de fermeture (en secondes).")]
        private float AnimationDuration = 0.4f;
        
        [SerializeField, Tooltip("Courbe d'accélération de la fermeture.")]
        private AnimationCurve CloseCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        
        [SerializeField, Tooltip("Temps d'attente (écran noir) avant de couper le jeu.")]
        private float WaitBeforeQuit = 0.15f;


        // --- Lifecycle ---

        public override void Initialize()
        {
            bIsPopup = true;
            
            base.Initialize();
            
            if (ConfirmButton != null)
            {
                ConfirmButton.onClick.AddListener(OnConfirmPowerOff);
            }
                
            if (CancelButton != null)
            {
                CancelButton.onClick.AddListener(OnCancelPowerOff);
            }
        }

        public override void Show()
        {
            base.Show();
            Effects.UIParallax.bIsGlobalParallaxPaused = true;
        }

        public override void Hide()
        {
            base.Hide();
            Effects.UIParallax.bIsGlobalParallaxPaused = false;
        }


        // --- Event Handlers ---

        private void OnConfirmPowerOff()
        {
            Debug.Log("[PowerOffPanel] Extinction du téléphone en cours...");
            StartCoroutine(PlayPowerOffAnimation());
        }


        private IEnumerator PlayPowerOffAnimation()
        {
            // 1. Bloquer les inputs
            if (CanvasGroupRef != null)
                CanvasGroupRef.interactable = false;

            // 2. Créer un conteneur
            Canvas rootCanvas = GetComponentInParent<Canvas>().rootCanvas;
            Transform overlayParent = rootCanvas != null ? rootCanvas.transform : transform;

            GameObject overlayObj = new GameObject("ShutdownOverlay");
            overlayObj.transform.SetParent(overlayParent, false);
            overlayObj.transform.SetAsLastSibling();

            RectTransform overlayRect = overlayObj.AddComponent<RectTransform>();
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.sizeDelta = Vector2.zero;
            overlayRect.anchoredPosition = Vector2.zero;

            // 3. Création panneaux noirs
            RectTransform topPanel = CreateBlackPanel("Top", new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), overlayRect);
            RectTransform botPanel = CreateBlackPanel("Bottom", new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0), overlayRect);
            RectTransform leftPanel = CreateBlackPanel("Left", new Vector2(0, 0), new Vector2(0, 1), new Vector2(0, 0.5f), overlayRect);
            RectTransform rightPanel = CreateBlackPanel("Right", new Vector2(1, 0), new Vector2(1, 1), new Vector2(1, 0.5f), overlayRect);

            float duration = AnimationDuration;
            float elapsed = 0f;
            
            float targetHeight = overlayRect.rect.height / 2f;
            float targetWidth = overlayRect.rect.width / 2f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = CloseCurve.Evaluate(elapsed / duration);

                topPanel.sizeDelta = new Vector2(0, targetHeight * t);
                botPanel.sizeDelta = new Vector2(0, targetHeight * t);
                leftPanel.sizeDelta = new Vector2(targetWidth * t, 0);
                rightPanel.sizeDelta = new Vector2(targetWidth * t, 0);

                yield return null;
            }

            topPanel.sizeDelta = new Vector2(0, targetHeight);
            botPanel.sizeDelta = new Vector2(0, targetHeight);
            leftPanel.sizeDelta = new Vector2(targetWidth, 0);
            rightPanel.sizeDelta = new Vector2(targetWidth, 0);

            yield return new WaitForSecondsRealtime(WaitBeforeQuit);

            // 5. Extinction du jeu
            #if UNITY_EDITOR
                EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }

        private RectTransform CreateBlackPanel(string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Transform parent)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            
            RectTransform rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.sizeDelta = Vector2.zero;
            rect.anchoredPosition = Vector2.zero;

            Image img = obj.AddComponent<Image>();
            img.color = Color.black;
            img.raycastTarget = true;

            return rect;
        }


        private void OnCancelPowerOff()
        {
            UIManager.Instance.CloseCurrentPanel();
        }
        
        
        private void OnDestroy()
        {
            if (ConfirmButton != null)
            {
                ConfirmButton.onClick.RemoveListener(OnConfirmPowerOff);
            }
                
            if (CancelButton != null)
            {
                CancelButton.onClick.RemoveListener(OnCancelPowerOff);
            }
        }
    }
}
