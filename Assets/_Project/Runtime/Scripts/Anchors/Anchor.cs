using UnityEngine;

namespace Metroma
{
    public class Anchor<T> : ScriptableObject
    {
        public T Target { get; set; }
    }
}
