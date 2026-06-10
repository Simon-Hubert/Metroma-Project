using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Metroma.Core;


namespace Metroma.UI.Panels
{
    public class AlarmPanel : UIPanel
    {
        [Header("UI Elements")]
        [SerializeField, Tooltip("Texte affichant l'heure in-game actuelle.")]
        private TMP_Text TimeText;
        
        [SerializeField, Tooltip("Bouton unique pour interagir avec l'alarme (l'éteindre).")]
        private Button InteractButton;

        public static event System.Action OnAlarmInteracted;


        public override void Initialize()
        {
            bIsPopup = false;
            
            base.Initialize();

            if (InteractButton != null)
                InteractButton.onClick.AddListener(OnInteractClicked);
        }

        public override void Show()
        {
            base.Show();
            
            AlarmManager.OnMinuteChanged += UpdateTimeDisplay;
            
            if (AlarmManager.Instance != null)
            {
                UpdateTimeDisplay(AlarmManager.Instance.CurrentHour, AlarmManager.Instance.CurrentMinute);
            }
        }

        public override void Hide()
        {
            base.Hide();
            AlarmManager.OnMinuteChanged -= UpdateTimeDisplay;
        }


        private void UpdateTimeDisplay(int hour, int minute)
        {
            if (TimeText != null)
            {
                TimeText.text = $"{hour:D2}:{minute:D2}";
            }
        }


        private void OnInteractClicked()
        {
            Debug.Log("[AlarmPanel] Alarme arrêtée par le joueur !");
            
            if (AlarmManager.Instance != null)
                AlarmManager.Instance.StopAlarm();
                
            StopPhoneVibration();
            
            // On prévient le reste du jeu (ou la cinématique) que le joueur a interagi
            OnAlarmInteracted?.Invoke();
            
            UIManager.Instance.CloseCurrentPanel();
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
