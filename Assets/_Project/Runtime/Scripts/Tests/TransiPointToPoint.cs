using System;
using UnityEngine;

namespace Metroma
{
    public class TransiPointToPoint : MonoBehaviour
    {
        [SerializeField] private Transform _toMove;
        [SerializeField] private Transform _target;
        [SerializeField] private float _speed = 10f;
        [SerializeField] private float _rotationSpeed = 10f;
        
        private bool _canMove = false;
        
        public void Play() => _canMove = true;
        public void Stop() => _canMove = false;

        private void Update()
        {
            if (_canMove && Vector3.Distance(_toMove.position, _target.position) > 0.1f)
            {
                _toMove.position += new Vector3(_speed * Time.deltaTime, 0, 0);
                _toMove.eulerAngles += new Vector3(0, 0, -_rotationSpeed * Time.deltaTime);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (_toMove && _target)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(_toMove.position, _target.position); 
            }
        }
    }
}
