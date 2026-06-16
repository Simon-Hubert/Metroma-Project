using UnityEngine;

namespace Metroma
{
    public class FullscreenImageAnimSequence : ASequencable
    {
        [SerializeField] private Sprite[] _frames;
        [SerializeField] private float _duration;
        [SerializeField] private float _framerate = 10;
        [SerializeField] private bool _looping;

        private int _currentFrame = -1;
        
        public override async Awaitable ExecuteAsync() {
            float t = 0;
            FullScreenImage.Show(_frames[0]);
            while (t < _duration) {
                await Awaitable.EndOfFrameAsync();
                t += Time.deltaTime;
                SetFrame(t);
            }
            FullScreenImage.Hide();
        }
        
        private void SetFrame(float t) {
            int frame =  Mathf.FloorToInt(t * _framerate);
            
            if (_looping) {
                frame %= _frames.Length;
            }
            else {
                frame = Mathf.Clamp(frame, 0, _frames.Length-1);
            }
            
            if (frame == _currentFrame) return;
            
            _currentFrame = frame;
            FullScreenImage.Show(_frames[frame]);
        }

        public override bool RequirementsValidated() {
            return _duration > 0 && _framerate > 0;
        }

        public void OnValidate() {
            if (!RequirementsValidated()) return;
        }

#if UNITY_EDITOR
        public override InspectorColor GetInspectorColor() {
            return new InspectorColor
            {
                Background = ColorHelpers.ColorFromOKLCH(BackgroundLightness, CHROMA, 260f),
                Text = ColorHelpers.ColorFromOKLCH(TextLightness, CHROMA, 260f),
            };
        }
#endif
    }
}
