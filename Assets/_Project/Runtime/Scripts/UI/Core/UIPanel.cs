using System;
using System.Collections;
using UnityEngine;


namespace Metroma.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class UIPanel : MonoBehaviour
    {
        [Header("Panel Transition Settings")]
        [SerializeField, Tooltip("Durée du fondu (Fade In / Fade Out) en secondes.")]
        protected float FadeDuration = 0.25f;

        [SerializeField, Tooltip("Si vrai, ce panel s'affichera par-dessus l'écran actuel sans le faire disparaître.")]
        protected bool bIsPopup = false;

        public bool IsPopup => bIsPopup;

        [Header("Gamepad Navigation")]
        [SerializeField, Tooltip("Bouton sélectionné par défaut quand on ouvre ce menu.")]
        public UnityEngine.UI.Selectable FirstSelected;

        [HideInInspector]
        public GameObject LastSelected;

        protected CanvasGroup CanvasGroupRef;
        private Coroutine FadeCoroutine;
        
        public event Action OnPanelShown;
        public event Action OnPanelHidden;


        public virtual void Initialize()
        {
            CanvasGroupRef = GetComponent<CanvasGroup>();
            CanvasGroupRef.alpha = 0f;
            CanvasGroupRef.interactable = false;
            CanvasGroupRef.blocksRaycasts = false;
        }


        public virtual void Show()
        {
            gameObject.SetActive(true);

            var juicyEntrances = GetComponentsInChildren<Metroma.UI.Effects.JuicyEntrance>();
            foreach (var entrance in juicyEntrances)
            {
                entrance.Play();
            }

            if (FadeCoroutine != null)
                StopCoroutine(FadeCoroutine);
    
            FadeCoroutine = StartCoroutine(FadeRoutine(1f, true));
        }


        public virtual void Hide()
        {
            if (FadeCoroutine != null)
                StopCoroutine(FadeCoroutine);

            FadeCoroutine = StartCoroutine(FadeRoutine(0f, false));
        }


        public virtual void OnFocus()
        {
            CanvasGroupRef.interactable = true;
            CanvasGroupRef.blocksRaycasts = true;
        }


        public virtual void OnDefocus()
        {
            CanvasGroupRef.interactable = false;
            CanvasGroupRef.blocksRaycasts = false;
        }


        private IEnumerator FadeRoutine(float TargetAlpha, bool bIsShowing)
        {
            UIManager.Instance.SetTransitioningState(true);

            float StartAlpha = CanvasGroupRef.alpha;
            float Elapsed = 0f;

            while (Elapsed < FadeDuration)
            {
                Elapsed += Time.unscaledDeltaTime;
                CanvasGroupRef.alpha = Mathf.Lerp(StartAlpha, TargetAlpha, Elapsed / FadeDuration);
                yield return null;
            }

            CanvasGroupRef.alpha = TargetAlpha;
            
            if (bIsShowing)
            {
                OnPanelShown?.Invoke();
            }
            else
            {
                gameObject.SetActive(false);
                OnPanelHidden?.Invoke();
            }

            UIManager.Instance.SetTransitioningState(false);
        }
    }
}
