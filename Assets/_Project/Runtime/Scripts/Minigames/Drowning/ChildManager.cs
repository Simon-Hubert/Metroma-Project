using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System.Numerics;
using Vector2 = UnityEngine.Vector2;

namespace Metroma
{
    public class ChildManager : MonoBehaviour
    {
        [SerializeField] private Transform _player;
        public Transform GetPlayer => _player;
        public Vector2 GetPlayerPos => _player ? _player.position : Vector2.zero;
        
        [SerializeField] private ConditionalEvent _endCondition;
        [Space(7)]
        [SerializeField] private List<ChildBehaviour> _childs = new List<ChildBehaviour>();
        
        
    }
}
