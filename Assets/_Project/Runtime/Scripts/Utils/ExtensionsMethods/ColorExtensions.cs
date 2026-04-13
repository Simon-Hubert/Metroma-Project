using UnityEngine;

namespace Metroma.Utils
{
    public static class StringExtensions
    {
        public static Color StringToColor(this string text)
        {
            Color color;
            int hashCode = 0;
            if (text != null) hashCode = text.GetHashCode();
            
            color.r = Mathf.Abs(hashCode % 255 / 255.0f);
            color.g = Mathf.Abs((int)(hashCode * 0.001) % 255 / 255.0f);
            color.b = Mathf.Abs((int)(hashCode * 0.000001) % 255 / 255.0f);
            color.a = 1;

            //Debug.Log($"StringToColor '{text}' -> hashcode: {hashCode} | R: {color.r} | G: {color.g} | B: {color.b}");
            return color;
        }
    }
}
