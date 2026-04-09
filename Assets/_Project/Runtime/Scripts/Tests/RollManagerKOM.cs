using System;
using UnityEngine;

namespace Metroma
{
    public class RollManagerKOM : MonoBehaviour
    {
        [SerializeField] private PlaceholderMovePositionAd _ad;
        [SerializeField] private Transform _coin;
        [SerializeField] private float _speed = 1f, _rotationSpeed = 10f;

        private bool _canvMove = false;

        private void Start()
        {
            _ad.OnMove += MoveCoin;
        }

        private void MoveCoin(bool canMove)
        {
            _canvMove = canMove;
        }

        private void Update()
        {
            if (_canvMove)
            {
                _coin.position += new Vector3(_speed * Time.deltaTime, 0, 0);
                _coin.eulerAngles += new Vector3(0, 0, -_rotationSpeed * Time.deltaTime);
            }
        }
    }
}
