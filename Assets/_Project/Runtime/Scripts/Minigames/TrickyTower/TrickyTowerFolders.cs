using System;
using NaughtyAttributes;
using UnityEngine;

namespace Metroma
{
    [Serializable]
    public struct TrickyTowerFoldersData
    {
        public float gravityScale;
        public float mass;
        public float maxVelocity;
        public float size;

        public TrickyTowerFoldersData(float gravity = 0.5f,  float m = 1f, float velocity = 10f, float scale = 1f) {
            gravityScale = gravity;
            mass = m;
            maxVelocity = velocity;
            size = scale;
        }
    }
    
    public class TrickyTowerFolders : MonoBehaviour
    {
        [SerializeField] private Rigidbody2D _rb2D;
        [SerializeField, ReadOnly] private TrickyTowerLogic _logic;
        [SerializeField, ReadOnly] private TrickyTowerFoldersData _data =  new TrickyTowerFoldersData(0.5f, 1f, 10f, 1f);
        [SerializeField, ReadOnly] private bool _isOnTable = false;
        public bool GetIsOnTable => _isOnTable;
        
        public void InitValues(TrickyTowerLogic logic, TrickyTowerFoldersData data) {
            if (_rb2D == null) {
                if (!TryGetComponent<Rigidbody2D>(out _rb2D)) {
                    Debug.LogError($"{name}'s TrickyTowerFolders : missing Rigidbody2D component", this);
                    return;
                }
            }
            if (logic == null) {
                Debug.LogError($"{name}'s TrickyTowerFolders : missing logic reference at Init", this);
                return;
            }
            
            _logic = logic;

            _isOnTable = false;
            _data = data;
            
            _rb2D.gravityScale = _data.gravityScale;
            _rb2D.mass = _data.mass * _data.size;
            transform.localScale = Vector3.one * _data.size;
        }

        private void Update() {
            if (_rb2D != null) _rb2D.linearVelocity = new Vector2(_rb2D.linearVelocity.x, Mathf.Clamp(_rb2D.linearVelocity.y, -_data.maxVelocity, Mathf.Infinity));
        }

        private void OnCollisionEnter2D(Collision2D other) {
            if (_logic != null && !_isOnTable) {
                _isOnTable = true;
                _logic.AddCapturedFolder();
                _logic.RemoveLaunchedFolder();
            }
        }
    }
}
