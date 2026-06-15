using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using NaughtyAttributes;


namespace Metroma.Audio
{
    [RequireComponent(typeof(Selectable))]
    public class UIButtonAudio : MonoBehaviour, IPointerEnterHandler, ISelectHandler, IPointerClickHandler
    {
        private Selectable TargetSelectable;

        [Header("Hover (Souris)")]
        [SerializeField] private bool bOverrideHover = false;
        [ShowIf("bOverrideHover"), SerializeField] private UnityEvent OnCustomHover;

        [Header("Select (Manette)")]
        [SerializeField] private bool bOverrideSelect = false;
        [ShowIf("bOverrideSelect"), SerializeField] private UnityEvent OnCustomSelect;

        [Header("Click (Validation)")]
        [SerializeField] private bool bOverrideClick = false;
        [ShowIf("bOverrideClick"), SerializeField] private UnityEvent OnCustomClick;



        private void Awake()
        {
            TargetSelectable = GetComponent<Selectable>();
        }


        // --- HOVER (Survol Souris) ---
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (TargetSelectable != null && !TargetSelectable.interactable)
                return;

            if (bOverrideHover)
            {
                OnCustomHover?.Invoke();
            }
            else if (UIAudioManager.Instance != null)
            {
                UIAudioManager.Instance.PlayGlobalHover();
            }
        }

        // --- SELECT (Navigation Manette) ---
        public void OnSelect(BaseEventData eventData)
        {
            if (TargetSelectable != null && !TargetSelectable.interactable)
                return;

            if (bOverrideSelect)
            {
                OnCustomSelect?.Invoke();
            }
            else if (UIAudioManager.Instance != null)
            {
                UIAudioManager.Instance.PlayGlobalSelect();
            }
        }

        // --- CLICK (Validation Souris / Manette) ---
        public void OnPointerClick(PointerEventData eventData)
        {
            if (TargetSelectable != null && !TargetSelectable.interactable) return;

            if (bOverrideClick)
                OnCustomClick?.Invoke();
            else if (UIAudioManager.Instance != null)
                UIAudioManager.Instance.PlayGlobalClick();
        }
        

        public void PlayCompleteSound()
        {
            if (UIAudioManager.Instance != null)
                UIAudioManager.Instance.PlayGlobalComplete();
        }
    }
}
