using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Metroma.Core;


namespace Metroma.UI.Panels
{
    public class AlarmPanel : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField, Tooltip("Texte affichant l'heure in-game actuelle.")]
        private TMP_Text TimeText;
        
        [SerializeField, Tooltip("Bouton unique pour interagir avec l'alarme (l'éteindre).")]
        private Button InteractButton;

        public static event System.Action OnAlarmInteracted;
        public static bool IsInteractionAllowed = true;
        public static Camera CurrentEventCamera;

        private CanvasGroup _canvasGroup;
        private Canvas _parentCanvas;


        private void Start()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _parentCanvas = GetComponentInParent<Canvas>();

            if (InteractButton != null)
            {
                InteractButton.onClick.AddListener(OnInteractClicked);
                InteractButton.gameObject.SetActive(false);
            }

            if (_canvasGroup != null)
            {
                _canvasGroup.interactable = false;
                _canvasGroup.blocksRaycasts = false;
            }

            AlarmManager.OnMinuteChanged += UpdateTimeDisplay;
            
            if (AlarmManager.Instance != null)
            {
                UpdateTimeDisplay(AlarmManager.Instance.CurrentHour, AlarmManager.Instance.CurrentMinute);
            }
        }

        public void ShowAlarmButton()
        {
            if (InteractButton != null)
            {
                InteractButton.gameObject.SetActive(true);
                InteractButton.interactable = IsInteractionAllowed;

                if (IsInteractionAllowed)
                {
                    FocusButtonForGamepad();
                    UpdateWorldSpaceCamera();
                    EnableCanvasInteraction();
                }
            }
        }

        private void FocusButtonForGamepad()
        {
            if (UnityEngine.EventSystems.EventSystem.current != null)
            {
                UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(InteractButton.gameObject);
            }
        }

        private void EnableCanvasInteraction()
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.interactable = true;
                _canvasGroup.blocksRaycasts = true;
            }
        }

        private void UpdateWorldSpaceCamera()
        {
            if (_parentCanvas != null && _parentCanvas.renderMode == RenderMode.WorldSpace)
            {
                if (CurrentEventCamera != null)
                {
                    _parentCanvas.worldCamera = CurrentEventCamera;
                }
                else if (Camera.main != null)
                {
                    _parentCanvas.worldCamera = Camera.main;
                }
            }
        }

        private void UpdateTimeDisplay(int hour, int minute)
        {
            if (TimeText != null)
            {
                TimeText.text = $"{hour:D2}:{minute:D2}";
            }
        }

        private async void OnInteractClicked()
        {
            if (InteractButton != null)
            {
                var juicy = InteractButton.GetComponent<Metroma.UI.Effects.JuicyButton>();
                if (juicy != null)
                {
                    await juicy.PlayClickEffectAsync();
                }
                InteractButton.gameObject.SetActive(false);
            }

            if (_canvasGroup != null)
            {
                _canvasGroup.interactable = false;
                _canvasGroup.blocksRaycasts = false;
            }

            Debug.Log("[AlarmPanel] Alarme arrêtée par le joueur !");
            
            if (AlarmManager.Instance != null)
                AlarmManager.Instance.StopAlarm();
                
            StopPhoneVibration();
            
            if (InteractButton != null)
                InteractButton.gameObject.SetActive(false);
            
            OnAlarmInteracted?.Invoke();
        }
        
        private void StopPhoneVibration()
        {
            PhoneController phone = FindObjectOfType<PhoneController>(true);
            if (phone != null)
            {
                phone.StopVibration();
            }
        }

        private void OnDestroy()
        {
            if (InteractButton != null)
                InteractButton.onClick.RemoveListener(OnInteractClicked);
                
            AlarmManager.OnMinuteChanged -= UpdateTimeDisplay;
        }
    }
}
