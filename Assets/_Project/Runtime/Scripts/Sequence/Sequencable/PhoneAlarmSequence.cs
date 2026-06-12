using UnityEngine;
using Metroma.Core;
using Metroma.UI;
using Metroma.UI.Panels;


namespace Metroma
{
    public class PhoneAlarmSequence : ASequencable
    {
        [Header("Cameras")]
        [SerializeField, Tooltip("La caméra qui filme le téléphone (à activer)")]
        private Camera _phoneCamera;

        [Header("Starting Time")]
        [SerializeField, Tooltip("Heure à laquelle l'horloge va commencer (ex: 06)")]
        private int _startHour = 6;
        
        [SerializeField, Tooltip("Minute à laquelle l'horloge va commencer (ex: 59). L'alarme sonnera automatiquement 1 minute plus tard !")]
        private int _startMinute = 59;

        [Header("Interaction")]
        [SerializeField, Tooltip("Le joueur doit-il interagir avec l'alarme pour la couper ?")]
        private bool _requirePlayerInteraction = true;
        
        [NaughtyAttributes.HideIf("_requirePlayerInteraction")]
        [SerializeField, Tooltip("Durée TOTALE de la séquence en secondes")]
        private float _totalSequenceDuration = 5f;

        [NaughtyAttributes.HideIf("_requirePlayerInteraction")]
        [SerializeField, Range(0.1f, 0.9f), Tooltip("Pourcentage de la durée totale alloué à la sonnerie (le reste sert à faire tourner l'horloge)")]
        private float _ringingTimeRatio = 0.3f;

        [Header("Timing (Interaction Requise)")]
        [NaughtyAttributes.ShowIf("_requirePlayerInteraction")]
        [SerializeField, Tooltip("Durée (en secondes) pour que l'horloge passe de l'heure de départ à l'heure de l'alarme")]
        private float _durationBeforeAlarm = 3.5f;

        [NaughtyAttributes.ShowIf("_requirePlayerInteraction")]
        [SerializeField, Tooltip("Délai (en secondes) à attendre après l'arrêt manuel de l'alarme avant de passer à la suite")]
        private float _delayAfterStop = 1f;

        private AwaitableCompletionSource _completionSource;
        private AwaitableCompletionSource _ringSource;
        private Camera _previousActiveCamera;
        
        private CursorLockMode _previousLockState;
        private bool _previousCursorVisible;


        public override async Awaitable ExecuteAsync() 
        {
            if (!RequirementsValidated())
                return;

            SaveAndDisableActiveCamera();
            UnlockCursorForInteraction();

            if (_phoneCamera != null)
            {
                _phoneCamera.enabled = true;
                AlarmPanel.CurrentEventCamera = _phoneCamera;
            }

            AlarmPanel panel = FindObjectOfType<AlarmPanel>(true);
            if (panel != null)
            {
                panel.SetSleepMode();
            }

            AlarmPanel.IsInteractionAllowed = _requirePlayerInteraction;

            _ringSource = new AwaitableCompletionSource();
            AlarmManager.OnAlarmRinging += OnAlarmStartedToRing;

            float timeBeforeAlarm = _requirePlayerInteraction ? _durationBeforeAlarm : (_totalSequenceDuration * (1f - _ringingTimeRatio));

            if (AlarmManager.Instance != null)
            {
                int startTotalMinutes = _startHour * 60 + _startMinute;
                int alarmTotalMinutes = startTotalMinutes + 1;
                
                int alarmHour = (alarmTotalMinutes / 60) % 24;
                int alarmMinute = alarmTotalMinutes % 60;

                float gameSecondsDiff = 60f;
                float requiredSpeed = gameSecondsDiff / timeBeforeAlarm;

                AlarmManager.Instance.SetTime(_startHour, _startMinute);
                AlarmManager.Instance.SetAlarm(alarmHour, alarmMinute);
                AlarmManager.Instance.TimeSpeedMultiplier = requiredSpeed;
            }
            else
            {
                _ringSource.TrySetResult();
            }

            await _ringSource.Awaitable;

            // Alarme sonne
            if (AlarmManager.Instance != null)
            {
                AlarmManager.Instance.TimeSpeedMultiplier = 0f;
            }

            _completionSource = new AwaitableCompletionSource();
            
            if (_requirePlayerInteraction)
            {
                AlarmPanel.OnAlarmInteracted += OnPlayerInteracted;
            }
            else
            {
                float ringingDuration = _totalSequenceDuration * _ringingTimeRatio;
                
                if (ringingDuration > 0f)
                    await Awaitable.WaitForSecondsAsync(ringingDuration);
                    
                ForceStopAlarm();
                EndSequence();
                _completionSource.TrySetResult();
            }

            await _completionSource.Awaitable;
        }

        private void SaveAndDisableActiveCamera()
        {
            _previousActiveCamera = null;
            foreach (Camera cam in Camera.allCameras)
            {
                if (_phoneCamera != null && cam == _phoneCamera)
                    continue;

                _previousActiveCamera = cam;
                _previousActiveCamera.enabled = false;
                break;
            }
        }

        private void UnlockCursorForInteraction()
        {
            _previousLockState = Cursor.lockState;
            _previousCursorVisible = Cursor.visible;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void OnAlarmStartedToRing()
        {
            AlarmManager.OnAlarmRinging -= OnAlarmStartedToRing;
            _ringSource.TrySetResult();
        }

        private async void OnPlayerInteracted()
        {
            AlarmPanel.OnAlarmInteracted -= OnPlayerInteracted;
            ForceStopAlarm();
            
            if (_delayAfterStop > 0f)
                await Awaitable.WaitForSecondsAsync(_delayAfterStop);
                
            EndSequence();
            _completionSource.TrySetResult();
        }

        private void ForceStopAlarm()
        {
            if (AlarmManager.Instance != null)
            {
                AlarmManager.Instance.StopAlarm();
            }
                
            PhoneController phone = FindObjectOfType<PhoneController>(true);
            if (phone != null)
            {
                phone.StopVibration();
            }
        }

        private void EndSequence()
        {
            if (AlarmManager.Instance != null)
            {
                AlarmManager.Instance.TimeSpeedMultiplier = 0f;
            }

            if (_phoneCamera != null)
                _phoneCamera.enabled = false;

            if (_previousActiveCamera != null)
                _previousActiveCamera.enabled = true;
                
            Cursor.lockState = _previousLockState;
            Cursor.visible = _previousCursorVisible;

            PhoneController phone = FindObjectOfType<PhoneController>(true);
            if (phone != null)
            {
                phone.HidePhone();
            }
                
            AlarmPanel.IsInteractionAllowed = true;
        }

        public override bool RequirementsValidated() 
        {
            return _phoneCamera != null;
        }

        private void OnValidate() 
        {
            int alarmTotalMinutes = (_startHour * 60 + _startMinute) + 1;
            int alarmHour = (alarmTotalMinutes / 60) % 24;
            int alarmMinute = alarmTotalMinutes % 60;
            
            name = $"Phone Alarm ({alarmHour:D2}:{alarmMinute:D2}) with Cam Switch";
        }
        
#if UNITY_EDITOR
        public override InspectorColor GetInspectorColor() 
        {
            return new InspectorColor
            {
                Background = ColorHelpers.ColorFromOKLCH(BackgroundLightness, CHROMA, 120f),
                Text = ColorHelpers.ColorFromOKLCH(TextLightness, CHROMA, 120f),
            };
        }
#endif
    }
}
