using NaughtyAttributes;
using UnityEngine;

namespace Metroma
{
    public class AnimTriggerSequence : ASequencable
    {
        [SerializeField] private Animator _animator;
        [SerializeField, AnimatorParam("_animator")] private string _triggerId;
        
        public override async Awaitable ExecuteAsync() {
            if (!RequirementsValidated()) return;
            _animator.SetTrigger(_triggerId);
        }

        public override bool RequirementsValidated() {
            return _animator && _triggerId.Length > 0;
        }

        private void OnValidate() {
            if (!RequirementsValidated()) return;
            name = $"Set {_triggerId} on {_animator.name}";
        }
        
#if UNITY_EDITOR
        public override InspectorColor GetInspectorColor() {
            return new InspectorColor
            {
                Background = ColorHelpers.ColorFromOKLCH(BackgroundLightness, CHROMA, 300f),
                Text = ColorHelpers.ColorFromOKLCH(TextLightness, CHROMA, 300f),
            };
        }
#endif
    }
}
