using Metroma.Utils;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

namespace Metroma
{
    public class StringExtensionTest : MonoBehaviour
    {
        [SerializeField] private string text;
        [SerializeField, ReadOnly] private Color color;

        [Button]
        public void TestTextToColor()
        {
            color = text.StringToColor();
        }
    }
}
