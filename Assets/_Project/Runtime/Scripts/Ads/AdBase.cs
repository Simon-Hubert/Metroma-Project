using System;
using UnityEngine;

namespace Metroma
{
    public class AdBase : MonoBehaviour
    {
        [SerializeField] private Block[] _blocks;
        
        private int _currentBlockIndex = -1;
        public int CurrentBlockIndex => _currentBlockIndex;
        
        private static int      _pendingBlockIndex   = -1;
        private static string[] _runtimeSandboxPaths = null;
        
        public static bool HasRuntimeSandbox => _runtimeSandboxPaths != null;
        
        public static void SetRuntimeSandbox(int index, Block[] blocks)
        {
            _pendingBlockIndex = index;
            if (blocks == null)
            {
                _runtimeSandboxPaths = null;
                return;
            }

            _runtimeSandboxPaths = new string[blocks.Length];
            for (int i = 0; i < blocks.Length; i++)
            {
                _runtimeSandboxPaths[i] = blocks[i] != null ? GetGameObjectPath(blocks[i].gameObject) : "";
            }
        }
        
        public static void ClearRuntimeSandbox()
        {
            _runtimeSandboxPaths = null;
        }

        public Block[] GetSandboxBlocks()
        {
            if (_runtimeSandboxPaths == null) return null;

            Block[] blocks = new Block[_runtimeSandboxPaths.Length];
            for (int i = 0; i < _runtimeSandboxPaths.Length; i++)
            {
                if (string.IsNullOrEmpty(_runtimeSandboxPaths[i])) continue;
                var go = GameObject.Find(_runtimeSandboxPaths[i]);
                if (go != null) blocks[i] = go.GetComponent<Block>();
            }
            return blocks;
        }

        private static string GetGameObjectPath(GameObject obj)
        {
            string path = obj.name;
            Transform current = obj.transform.parent;
            while (current != null)
            {
                path = current.name + "/" + path;
                current = current.parent;
            }
            return path;
        }

        public static void SetPendingBlockIndex(int index)
        {
            _pendingBlockIndex = index;
        }
        
        public event Action OnAdStarted;
        public event Action OnAdEnded;
        
        private void Start()
        {
            if (_runtimeSandboxPaths != null)
            {
                _blocks = GetSandboxBlocks();
                Debug.Log($"[AdBase] Injection de l'ordre Sandbox ({_blocks.Length} blocs).");
            }

            if (_pendingBlockIndex >= 0 && _pendingBlockIndex < _blocks.Length)
            {
                int target = _pendingBlockIndex;
                _pendingBlockIndex = -1;
                OnAdStarted?.Invoke();
                PlayBlockDirectly(target);
            }
            else
            {
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
            _currentBlockIndex = -1;
            OnAdEnded?.Invoke();
        }

        private void GoNextBlock()
        {
            int next = _currentBlockIndex + 1;

            if (next >= _blocks.Length)
            {
                End();
                return;
            }

            if (!_blocks[next])
            {
                Debug.LogWarning($"[AdBase] Bloc null à l'index {next} dans {name}.");
                End();
                return;
            }

            _currentBlockIndex = next;
            _blocks[_currentBlockIndex].StartBlock();
            _blocks[_currentBlockIndex].OnBlockEnded += GoNextBlock;
        }
        
        private void PlayBlockDirectly(int index)
        {
            if (_currentBlockIndex >= 0 && _currentBlockIndex < _blocks.Length
                && _blocks[_currentBlockIndex] != null)
            {
                _blocks[_currentBlockIndex].OnBlockEnded -= GoNextBlock;
                _blocks[_currentBlockIndex].End();
            }

            _currentBlockIndex = index;
            _blocks[_currentBlockIndex].StartBlock();
            _blocks[_currentBlockIndex].OnBlockEnded += GoNextBlock;
            Debug.Log($"[AdBase] Démarrage direct du bloc {index} après reload.");
        }
        
        public void PlayBlock(int index)
        {
            if (index < 0 || index >= _blocks.Length)
            {
                Debug.LogWarning($"[AdBase] PlayBlock : index {index} hors limites.");
                return;
            }

            if (_currentBlockIndex >= 0 && _currentBlockIndex < _blocks.Length
                && _blocks[_currentBlockIndex] != null)
            {
                _blocks[_currentBlockIndex].OnBlockEnded -= GoNextBlock;
                _blocks[_currentBlockIndex].End();
            }

            _currentBlockIndex = index - 1;
            GoNextBlock();
        }

        public void AutoFetchChildBlocks()
        {
            _blocks = GetComponentsInChildren<Block>();
        }
    }
}
