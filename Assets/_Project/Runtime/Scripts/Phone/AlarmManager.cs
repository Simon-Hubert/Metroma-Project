using System;
using UnityEngine;
using Metroma.UI;
using Metroma.UI.Panels;
using NaughtyAttributes;


namespace Metroma.Core
{
    public class AlarmManager : MonoBehaviour
    {
        public static AlarmManager Instance { get; private set; }

        [Header("Time Settings")]
        [SerializeField, Tooltip("Heure de départ (0-23)")]
        private int StartHour = 10;
        
        [SerializeField, Tooltip("Minute de départ (0-59)")]
        private int StartMinute = 0;

        [SerializeField, Tooltip("Vitesse d'écoulement du temps. 1 = Temps réel. 60 = 1 minute passe en 1 seconde.")]
        public float TimeSpeedMultiplier = 1f;


        // --- État de l'Alarme ---
        private bool bIsAlarmSet = false;
        private int AlarmHour = 0;
        private int AlarmMinute = 0;
        private bool bSnoozeAllowed = false;
        private bool bIsRinging = false;

        // --- Temps en Jeu ---
        private float InternalSeconds = 0f;
        public int CurrentHour { get; private set; }
        public int CurrentMinute { get; private set; }

        public static event Action<int, int> OnMinuteChanged;

        [Header("Audio Events")]
        [SerializeField, Tooltip("Déclenché quand l'alarme commence à sonner (Pratique pour lancer un AudioSource)")]
        private UnityEngine.Events.UnityEvent OnAlarmStarted;
        
        [SerializeField, Tooltip("Déclenché quand le joueur éteint l'alarme (Pratique pour couper un AudioSource)")]
        private UnityEngine.Events.UnityEvent OnAlarmStopped;


        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            CurrentHour = StartHour;
            CurrentMinute = StartMinute;
        }

        private void Start()
        {
            OnMinuteChanged?.Invoke(CurrentHour, CurrentMinute);
        }

        private void Update()
        {
            // Avancer le temps
            InternalSeconds += Time.deltaTime * TimeSpeedMultiplier;

            if (InternalSeconds >= 60f)
            {
                InternalSeconds -= 60f;
                AddMinute();
            }
        }

        private void AddMinute()
        {
            CurrentMinute++;
            if (CurrentMinute >= 60)
            {
                CurrentMinute = 0;
                CurrentHour++;
                if (CurrentHour >= 24)
                {
                    CurrentHour = 0;
                }
            }

            OnMinuteChanged?.Invoke(CurrentHour, CurrentMinute);
            CheckAlarm();
        }


        public void SetAlarm(int hour, int minute)
        {
            AlarmHour = hour;
            AlarmMinute = minute;
            bIsAlarmSet = true;
            bIsRinging = false;
            
            Debug.Log($"[AlarmManager] Réveil programmé pour {hour:D2}:{minute:D2}");
        }

        private void CheckAlarm()
        {
            if (!bIsAlarmSet || bIsRinging) return;

            if (CurrentHour == AlarmHour && CurrentMinute == AlarmMinute)
            {
                TriggerAlarm();
            }
        }

        [Button("Test Force Trigger Alarm")]
        private void TriggerAlarm()
        {
            Debug.Log("[AlarmManager] DRING DRING ! Le réveil sonne !");
            bIsRinging = true;
            bIsAlarmSet = false;
            
            OnAlarmStarted?.Invoke();

            if (UIManager.Instance != null)
            {
                UIManager.Instance.OpenPanel<AlarmPanel>();
            }

            PhoneController phone = FindObjectOfType<PhoneController>(true);
            if (phone != null)
            {
                phone.ShowPhone();
                phone.VibratePhone();
            }
        }


        public void StopAlarm()
        {
            Debug.Log("[AlarmManager] Réveil éteint.");
            bIsRinging = false;
            bIsAlarmSet = false;
            
            OnAlarmStopped?.Invoke();
        }
    }
}
