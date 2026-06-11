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

        [Header("Alarm Settings")]
        [SerializeField, Tooltip("Heure de l'alarme")]
        private int _alarmHour = 10;
        
        [SerializeField, Tooltip("Minute de l'alarme")]
        private int _alarmMinute = 0;

        [Header("Interaction")]
        [SerializeField, Tooltip("Le joueur doit-il interagir avec l'alarme pour la couper ?")]
        private bool _requirePlayerInteraction = true;
        
        [NaughtyAttributes.HideIf("_requirePlayerInteraction")]
        [SerializeField, Tooltip("Durée (en secondes) avant l'arrêt automatique si pas d'interaction")]
        private float _autoStopDuration = 5f;

        [Header("Timing")]
        [SerializeField, Tooltip("Délai (en secondes) à attendre après l'arrêt de l'alarme avant de passer à la suite")]
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

            AlarmPanel.IsInteractionAllowed = _requirePlayerInteraction;

            // Lancement programme de l'Alarme
            _ringSource = new AwaitableCompletionSource();
            AlarmManager.OnAlarmRinging += OnAlarmStartedToRing;

            if (AlarmManager.Instance != null)
            {
                AlarmManager.Instance.SetAlarm(_alarmHour, _alarmMinute);
            }
            else
            {
                Debug.LogError("[PhoneAlarmSequence] No AlarmManager found in the scene.");
                OnAlarmStartedToRing(); 
            }

            await _ringSource.Awaitable;
            
            // L'alarme sonne
            if (_requirePlayerInteraction)
            {
                _completionSource = new AwaitableCompletionSource();
                AlarmPanel.OnAlarmInteracted += OnPlayerInteracted;
                await _completionSource.Awaitable;
            }
            else
            {
                await Awaitable.WaitForSecondsAsync(_autoStopDuration);
                ForceStopAlarm();
                
                if (_delayAfterStop > 0f)
                    await Awaitable.WaitForSecondsAsync(_delayAfterStop);
                    
                EndSequence();
            }
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
            if (_phoneCamera != null)
                _phoneCamera.enabled = false;

            if (_previousActiveCamera != null)
                _previousActiveCamera.enabled = true;
                
            Cursor.lockState = _previousLockState;
            Cursor.visible = _previousCursorVisible;
                
            AlarmPanel.IsInteractionAllowed = true;
        }

        public override bool RequirementsValidated() 
        {
            return _phoneCamera != null;
        }

        private void OnValidate() 
        {
            name = $"Phone Alarm ({_alarmHour:D2}:{_alarmMinute:D2}) with Cam Switch";
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
