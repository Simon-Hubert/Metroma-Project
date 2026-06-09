using NaughtyAttributes;
using UnityEngine;

namespace Metroma
{
    public class SwitchCameraSequence : ASequencable
    {
        [Header("Camera From")]
        [SerializeField] private CameraAnchor _cameraFrom;
        [SerializeField] private bool _activeA;
        [SerializeField] private bool _setTargetTexA;
        [SerializeField, ShowIf("_setTargetTexA")]
        private RenderTexture _texA;
        
        [Header("Camera To")]
        [SerializeField] private CameraAnchor _cameraTo;
        [SerializeField] private bool _activeB;
        [SerializeField] private bool _setTargetTexB;
        [SerializeField, ShowIf("_setTargetTexB")]
        private RenderTexture _texB;
        
        public override async Awaitable ExecuteAsync() {
            if (!RequirementsValidated()) return;
            Camera from =_cameraFrom.Target; 
            from.enabled = _activeA;
            Camera to = _cameraTo.Target;
            to.enabled = _activeB;
            if (_setTargetTexA) {
                from.targetTexture = _texA;
            }
            if (_setTargetTexB) {
                to.targetTexture = _texB;
            }
        }
        
        public override bool RequirementsValidated() {
            return _cameraFrom && _cameraTo;
        }

        private void OnValidate() {
            name = $"Switch from {_cameraFrom.name} to {_cameraTo.name}";
        }
        
#if UNITY_EDITOR
        public override InspectorColor GetInspectorColor() {
            return new InspectorColor
            {
                Background = ColorHelpers.ColorFromOKLCH(BackgroundLightness, CHROMA, 39f),
                Text = ColorHelpers.ColorFromOKLCH(TextLightness, CHROMA, 39f),
            };
        }
#endif
    }
}
