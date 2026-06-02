using UnityEngine;

namespace Metroma
{
    #if UNITY_EDITOR
    public struct InspectorColor
    {
        public Color Background;
        public Color Text;
    }
    #endif
    
    public abstract class ASequencable : MonoBehaviour
    {
        public abstract Awaitable ExecuteAsync();

        public virtual bool RequirementsValidated() => true;
        
#if UNITY_EDITOR
        protected const float CHROMA = 0.1005f;
        protected const float BackgroundLightness = 0.6513f;
        protected const float TextLightness = 0.25f;
        
        public virtual InspectorColor GetInspectorColor() {
            return new InspectorColor
            {
                Background = ColorHelpers.ColorFromHex("#373737"),
                Text = Color.white
            };
        }
#endif
    }
}
