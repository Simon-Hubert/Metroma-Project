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

        [Header("Background Automation")]
        [SerializeField, Tooltip("Le composant Image du fond du téléphone.")]
        private Image BackgroundImage;
        
        [SerializeField, Tooltip("Le sprite affiché quand le téléphone est en veille (avant l'alarme).")]
        private Sprite SleepBackgroundSprite;
        
        [SerializeField, Tooltip("Le sprite affiché quand l'alarme sonne (réveil).")]
        private Sprite AwakeBackgroundSprite;

        [Header("Lighting Automation")]
        [SerializeField, Tooltip("Liste des lumières (Lights) à allumer quand l'alarme sonne, et à éteindre en veille.")]
        private System.Collections.Generic.List<Light> AlarmLights;

        [Header("Text & Icon Automation")]
        [SerializeField, Tooltip("Texte affichant la date in-game.")]
        private TMP_Text DateText;
        
        [SerializeField, Tooltip("Composant Image de l'icône sur l'écran.")]
        private Image IconImage;

        [Space(10)]
        [SerializeField, Tooltip("Couleur du texte de l'heure en mode veille.")]
        private Color SleepTimeColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        
        [SerializeField, Tooltip("Couleur du texte de l'heure quand l'alarme sonne.")]
        private Color AwakeTimeColor = Color.white;

        [Space(10)]
        [SerializeField, Tooltip("Couleur du texte de la date en mode veille.")]
        private Color SleepDateColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        
        [SerializeField, Tooltip("Couleur du texte de la date quand l'alarme sonne.")]
        private Color AwakeDateColor = Color.white;

        [Space(10)]
        [SerializeField, Tooltip("Couleur de l'icône en mode veille.")]
        private Color SleepIconColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        
        [SerializeField, Tooltip("Couleur de l'icône quand l'alarme sonne.")]
        private Color AwakeIconColor = Color.white;

        [Header("UI States (Cinematic)")]
        [SerializeField, Tooltip("Déclenché quand le téléphone attend l'alarme (écran sombre, juste l'heure)")]
        private UnityEngine.Events.UnityEvent OnSleepModeEntered;
        
        [SerializeField, Tooltip("Déclenché quand l'alarme sonne (lumière, glow, apparition du bouton)")]
        private UnityEngine.Events.UnityEvent OnAwakeModeEntered;

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
            if (BackgroundImage != null && AwakeBackgroundSprite != null)
                BackgroundImage.sprite = AwakeBackgroundSprite;

            if (TimeText != null)
                TimeText.color = AwakeTimeColor;

            if (DateText != null)
                DateText.color = AwakeDateColor;
                
            if (IconImage != null)
                IconImage.color = AwakeIconColor;

            if (AlarmLights != null)
            {
                foreach (Light light in AlarmLights)
                {
                    if (light != null) light.enabled = true;
                }
            }

            OnAwakeModeEntered?.Invoke();
            
            if (InteractButton != null)
            {
                InteractButton.gameObject.SetActive(true);
                InteractButton.interactable = IsInteractionAllowed;

                if (IsInteractionAllowed)
                {
                    EnableCanvasInteraction();
                    UpdateWorldSpaceCamera();
                    FocusButtonForGamepad();
                }
            }
        }

        public void SetSleepMode()
        {
            if (InteractButton != null)
                InteractButton.gameObject.SetActive(false);
                
            if (BackgroundImage != null && SleepBackgroundSprite != null)
                BackgroundImage.sprite = SleepBackgroundSprite;
                
            if (TimeText != null)
                TimeText.color = SleepTimeColor;

            if (DateText != null)
                DateText.color = SleepDateColor;
                
            if (IconImage != null)
                IconImage.color = SleepIconColor;

            if (AlarmLights != null)
            {
                foreach (Light light in AlarmLights)
                {
                    if (light != null) light.enabled = false;
                }
            }
                
            OnSleepModeEntered?.Invoke();
        }

        private void FocusButtonForGamepad()
        {
            // Vérifie si une manette est branchée pour donner le focus
            if (UnityEngine.InputSystem.Gamepad.current != null && UnityEngine.EventSystems.EventSystem.current != null)
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
