using UnityEngine;


namespace Metroma
{
    public static class ColorHelpers
    {
        public static Color ColorFromHex(string hexValue) {
            ColorUtility.TryParseHtmlString(hexValue, out Color col);
            return col;
        }
        
        private static float linearToGamma(float c) {
            return c >= 0.0031308f ? 1.055f * Mathf.Pow(c, 1 / 2.4f) - 0.055f : 12.92f * c;
        }

        public static Color ColorFromOKLAB(float L, float a, float p) {
            float l = L + a * +0.3963377774f + p * 0.2158037573f;
            float m = L + a * -0.1055613458f + p * -0.0638541728f;
            float s = L + a * -0.0894841775f + p * 1.2914855480f;
            l = l*l*l; m = m*m*m; s = s*s*s;
            float r = l * +4.0767416621f + m * -3.3077115913f + s * +0.2309699292f;
            float g = l * -1.2684380046f + m * +2.6097574011f + s * -0.3413193965f;
            float b = l * -0.0041960863f + m * -0.7034186147f + s * +1.7076147010f;
            
            // Convert linear RGB values returned from oklab math to sRGB for our use before returning them:
            r = 255 * linearToGamma(r); g = 255 * linearToGamma(g); b = 255 * linearToGamma(b);
            r = Mathf.Clamp(r, 0, 255); g = Mathf.Clamp(g, 0, 255); b = Mathf.Clamp(b, 0, 255);
            r = r / 255f;
            g = g / 255f;
            b = b / 255f;
            return new Color(r,g,b);
        }

        public static Color ColorFromOKLCH(float L, float C, float H) {
            float a = C * Mathf.Cos(H);
            float b = C * Mathf.Sin(H);
            return ColorFromOKLAB(L, a, b);
        }
    } 
}

