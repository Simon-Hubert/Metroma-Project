using UnityEngine;
using UnityEngine.Events;

namespace Metroma
{
    public class UnityEventSequence : ASequencable
    {
        [SerializeField] private UnityEvent _event;
        

        public override async Awaitable ExecuteAsync() {
            _event?.Invoke();
        }
        
        public override bool RequirementsValidated() {
            return _event.GetPersistentEventCount() > 0;
        }

        private void OnValidate() {
            name = $"Invoke Event with {_event.GetPersistentEventCount()} binds";
        }
        
#if UNITY_EDITOR
        public override InspectorColor GetInspectorColor() {
            return new InspectorColor
            {
                Background = ColorHelpers.ColorFromOKLCH(BackgroundLightness, CHROMA, 247f),
                Text = ColorHelpers.ColorFromOKLCH(TextLightness, CHROMA, 247f),
            };
        }
#endif
    }
}
