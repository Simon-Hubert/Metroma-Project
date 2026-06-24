using System;
using UnityEngine;

namespace Metroma
{
    public class ActivateAnimationSequence : ASequencable
    {
        [SerializeField] private Animator _animator;
        [SerializeField] private string _parameter;
        
        public override async Awaitable ExecuteAsync() {
            if (!RequirementsValidated()) return;
            
            _animator.SetBool(_parameter, true);
        }

        public override bool RequirementsValidated() {
            return _animator != null;
        }

        public void OnValidate() {
            if (!RequirementsValidated()) return;
            name = $"Play {_animator.name} : _parameter";
        }

#if UNITY_EDITOR
        public override InspectorColor GetInspectorColor() {
            return new InspectorColor
            {
                Background = ColorHelpers.ColorFromOKLCH(BackgroundLightness, CHROMA, 47f),
                Text = ColorHelpers.ColorFromOKLCH(TextLightness, CHROMA, 47f),
            };
        }
#endif
    }
}
