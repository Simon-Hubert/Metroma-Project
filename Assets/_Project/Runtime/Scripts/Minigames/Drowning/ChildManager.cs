using UnityEngine;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine.Events;

namespace Metroma
{
    public class ChildManager : MonoBehaviour
    {
        [SerializeField] private Transform _player;
        [SerializeField] private Transform _poolBorder;
        public Transform GetPlayer => _player;
        public Vector2 GetPlayerPos => _player ? _player.position : Vector2.zero;
        public Transform GetPoolBorder => _poolBorder;
        
        [SerializeField] private ConditionalEvent _endCondition;
        [Space(7)]
        [SerializeField] private List<ChildBehaviour> _childs = new List<ChildBehaviour>();
        private int _nbOut = 0;
        [SerializeField] private UnityEvent OnComplete;

        private void Start() {
            PurgeList();
            _nbOut = 0;
        }

        public void OutChild() {
            _nbOut++;

            if (_nbOut >= _childs.Count) {
                _endCondition.Evaluate();
                OnComplete?.Invoke();
            }
        }

        [Button]
        private void PurgeList() {
            for (int i = 0; i < _childs.Count; i++) {
                if (_childs[i] == null) {
                    _childs.RemoveAt(i);
                    i--;
                }
            }
        }
    }
}
