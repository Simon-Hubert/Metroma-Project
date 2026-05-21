using System;
using UnityEngine;
using NaughtyAttributes;
using Random = UnityEngine.Random;

namespace Metroma
{
    public class TrickyTowerLogic : MonoBehaviour
    {
        [SerializeField] private bool _isLoopActive = false;
        public bool SetGameLoop(bool active) => _isLoopActive = active;
        
        [Header("Spawn")]
        [SerializeField] private float _spawnHeight = 8f;
        [SerializeField] private float _spawnRadius = 8f;
        [Space(7)]
        [SerializeField] private Vector2 _spawnTime = Vector2.one;
        private float _currentWait = 0f;

        [Header("Folders")]
        [SerializeField] private Transform _folderParent;
        [SerializeField] private GameObject _folderPrefab;
        [Space(7)]
        [SerializeField] private int _maxNbLaunchedFolders = 3;
        [SerializeField, ReadOnly] private int _nbLaunchedFolders = 0;
        public void AddLaunchedFolder() => _nbLaunchedFolders++;
        public void RemoveLaunchedFolder() => _nbLaunchedFolders = Mathf.Max(_nbLaunchedFolders - 1, 0);
        [SerializeField, ReadOnly, ConditionParam] private int _nbCapturedFolders = 0;
        public void AddCapturedFolder() => _nbCapturedFolders++;
        [SerializeField, ReadOnly, ConditionParam] private int _nbDeadFolders = 0;
        public void AddDeadFolder() => _nbDeadFolders++;
        [Space(7)]
        [SerializeField, Min(0)] private float _foldersMass = 1;
        [SerializeField, Min(0)] private Vector2 _foldersGravityScale = Vector2.one * 0.5f;
        [SerializeField, Min(0)] private Vector2 _foldersSize = Vector2.one;
        [SerializeField, Min(0)] private Vector2 _foldersMaxVelocity = Vector2.one * 10;

        private void OnValidate() {
            if (_foldersGravityScale.y < _foldersGravityScale.x) _foldersGravityScale.y = _foldersGravityScale.x;
            if (_foldersSize.y < _foldersSize.x) _foldersSize.y = _foldersSize.x;
            if (_foldersMaxVelocity.y < _foldersMaxVelocity.x) _foldersMaxVelocity.y = _foldersMaxVelocity.x;
            if (_spawnTime.y < _spawnTime.x) _spawnTime.y = _spawnTime.x;
        }

        private void Start()
        {
            _currentWait = _spawnTime.y;
        }

        private void Update() {
            if (!_isLoopActive) return;

            if (_nbLaunchedFolders < _maxNbLaunchedFolders) {
                if (_currentWait > 0f) {
                    _currentWait -= Time.deltaTime;
                }
                else
                {
                    GameObject folder = Instantiate(_folderPrefab, _folderParent);
                    folder.transform.localPosition = new Vector2(Random.Range(-_spawnRadius, _spawnRadius), _spawnHeight);

                    if (folder.TryGetComponent(out TrickyTowerFolders folderScript)) {
                        TrickyTowerFoldersData data = new TrickyTowerFoldersData(
                            Random.Range(_foldersGravityScale.x, _foldersGravityScale.y),
                            _foldersMass,
                            Random.Range(_foldersMaxVelocity.x, _foldersMaxVelocity.y),
                            Random.Range(_foldersSize.x, _foldersSize.y)
                            );
                        
                        folderScript.InitValues(this, data);
                    }
                    else {
                        Debug.LogError($"{name}'s TrickyTowerLogic : folder could not be created, missing 'TrickyTowerFolders' component");
                        Destroy(folder);
                        return;
                    }
                    
                    AddLaunchedFolder();
                    _currentWait = Random.Range(_spawnTime.x, _spawnTime.y);
                }
            }
        }
    }
}
