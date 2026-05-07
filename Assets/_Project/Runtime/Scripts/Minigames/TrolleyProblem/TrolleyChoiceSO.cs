using System;
using UnityEngine;
using System.Collections.Generic;

namespace Metroma
{
    [CreateAssetMenu(fileName = "TrolleyChoiceSO", menuName = "Metroma/Scriptable Objects/TrolleyChoiceSO")]
    [Serializable]
    public class TrolleyChoiceSO : ScriptableObject {
        public string id;
        public Sprite background;
        public Sprite illustration;

        public string name;
        public List<string> elements;
    }
}
