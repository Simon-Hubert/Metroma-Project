using UnityEngine;
using UnityEngine.InputSystem;

namespace Metroma
{
    public class GamepadVibrationSequence : ASequencable
    {
        [SerializeField] private float _duration = 0.2f;
        [SerializeField, Range(0f, 1f)] private float _lowFrequency = 0.5f;
        [SerializeField, Range(0f, 1f)] private float _highFrequency = 0.5f;
        [SerializeField] private bool _waitForCompletion = true;

        public override async Awaitable ExecuteAsync()
        {
            if (!RequirementsValidated())
                return;

            Gamepad gamepad = Gamepad.current;
            gamepad.SetMotorSpeeds(_lowFrequency, _highFrequency);

            if (_waitForCompletion)
            {
                float time = _duration;
                while (time > 0)
                {
                    time -= Time.deltaTime;
                    await Awaitable.NextFrameAsync();
                }

                if (gamepad != null)
                    gamepad.SetMotorSpeeds(0f, 0f);
            }
            else
            {
                StopVibrationAsync(gamepad, _duration);
            }
        }

        private async void StopVibrationAsync(Gamepad gamepad, float duration)
        {
            float time = duration;
            while (time > 0)
            {
                time -= Time.deltaTime;
                await Awaitable.NextFrameAsync();
            }

            if (gamepad != null)
                gamepad.SetMotorSpeeds(0f, 0f);
        }

        public override bool RequirementsValidated()
        {
            return _duration > 0f && Gamepad.current != null;
        }

        private void OnDisable()
        {
            if (Gamepad.current != null)
                Gamepad.current.SetMotorSpeeds(0f, 0f);
        }

        private void OnValidate()
        {
            name = $"Vibrate Gamepad ({_lowFrequency:F1}, {_highFrequency:F1}) for {_duration} sec";
        }
        
#if UNITY_EDITOR
        public override InspectorColor GetInspectorColor()
        {
            return new InspectorColor
            {
                Background = ColorHelpers.ColorFromOKLCH(BackgroundLightness, CHROMA, 350f),
                Text = ColorHelpers.ColorFromOKLCH(TextLightness, CHROMA, 350f),
            };
        }
#endif
    }
}
