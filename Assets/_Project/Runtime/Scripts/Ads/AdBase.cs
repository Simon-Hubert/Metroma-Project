using System;
using UnityEngine;

namespace Metroma
{
    public class AdBase : MonoBehaviour
    {
        [SerializeField] private Block[] _blocks;
        [SerializeField] private bool _activateOnStart;

        private int _currentBlock;

        public event Action OnAdStarted;
        public event Action OnAdEnded;

        private void Start() {
            if (_activateOnStart) {
                StartAd();
            }
        }

        public virtual void StartAd()
        {
            Debug.Log($"{name} started !");
            OnAdStarted?.Invoke();
            GoNextBlock();
        }
        
        protected virtual void End()
        {
            OnAdEnded?.Invoke();
        }

        private void GoNextBlock() {
            if (_currentBlock >= _blocks.Length) {
                End();
                return;
            }

            if (!_blocks[_currentBlock]) {
                Debug.Log($"null block in {name}.");
                End();
                return;
            }
            
            _blocks[_currentBlock].StartBlock();
            _blocks[_currentBlock].OnBlockEnded += GoNextBlock;
            _currentBlock++;
        }
    }
}
