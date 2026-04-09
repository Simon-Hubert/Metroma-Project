using System;
using UnityEngine;
using UnityEngine.Events;

namespace Metroma
{
    public class PlaceholderMovePositionAd : AdBase
    {
        private readonly WinCond _winCond = new WinCond();
        [SerializeField] private Transform _target;
        [SerializeField] private float _delta = 0.1f;
        [SerializeField] private Transform _currentPosition; //a terme il s'agira de la position du controllable
        private bool _isMoving = false, _won = false;

        [SerializeField] private UnityEvent<bool> _move;
        [SerializeField] private UnityEvent UnityOnAdEnd;
        
        public event Action<bool> OnMove;
        
        public Vector3 CurrentPosition => _currentPosition.position;
        public Vector3 Target => _target.position;
        
        private class WinCond : ICondition<PlaceholderMovePositionAd>
        {
            public bool Evaluate(PlaceholderMovePositionAd context) {
                return Vector3.Distance(context._currentPosition.localPosition, context._target.localPosition) <= context._delta;
            }
        }
        
        public override void StartAd() {
            if (!controllables[0]) {
                Debug.Log("il y a pas de controllable");
                return;
            }
            InputManager.instance.AddControllable(controllables[0]);
            controllables[0].OnMoveStart += Move;
            controllables[0].OnMoveEnd += StopMove;
            controllables[0].IsActive = true;
        }

        private void OnDisable() {
            controllables[0].OnActionStart -= Move;
            InputManager.instance.PurgeControllables();
        }

        protected override void OnAdEnd() {
            base.OnAdEnd();
            controllables[0].OnMoveStart -= Move;
            controllables[0].OnMoveEnd -= StopMove;
        }

        private void FixedUpdate()
        {
            if (_isMoving)
            {
                if (_winCond.Evaluate(this) && !_won) {
                    controllables[0].OnMoveStart -= Move;//bullshit pour stop les inputs sans forcément invoke l'event qui notifie que la pub est finie
                    controllables[0].OnMoveEnd -= StopMove;//bullshit pour stop les inputs sans forcément invoke l'event qui notifie que la pub est finie
                    _won = true;
                    UnityOnAdEnd?.Invoke();
                }
            }
        }

        private void Move() {
            if (!_isMoving)
            {
                _isMoving = true;
                _move?.Invoke(true);
                OnMove?.Invoke(true);
            }
        }

        private void StopMove()
        {
            if (_isMoving)
            {
                _isMoving = false;
                _move?.Invoke(false);
                OnMove?.Invoke(false);
            }
        }
    }
}
