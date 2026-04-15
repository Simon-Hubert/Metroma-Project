using System;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Metroma.Inputs
{
    [Serializable] 
    public struct GameplayInputsData
    {
        /// <summary>
        /// Move input value clamped between -1 & 1
        /// </summary>
        public Vector2 move { get; }
        /// <summary>
        /// Direction of the move input as a normalized vector
        /// </summary>
        public Vector2 moveDir => move.normalized;
        /// <summary>
        /// Time since the move input was used
        /// </summary>
        public float moveAFK { get; }
        
        /// <summary>
        /// Action input value
        /// </summary>
        public bool action { get; }
        /// <summary>
        /// Time since the move input was used
        /// </summary>
        public float actionAFK { get; }

        public float timeSinceLastInput => moveAFK < actionAFK ? moveAFK : actionAFK;
        
        public GameplayInputsData(Vector2 newMove, float newMoveAFK, bool newAction, float newActionAFK)
        {
            move = newMove;
            moveAFK = newActionAFK;
            action = newAction;
            actionAFK = newActionAFK;
        }
    }
    
    public delegate void CallBack();
    
    public class CaptureInputs : MonoBehaviour
    {
        private MetromaActions _inputAction;

        [SerializeField, ReadOnly] private Vector2 _move;
        [SerializeField, ReadOnly] private float _moveAFK;
        [Space(7)]
        [SerializeField, ReadOnly] private bool _action;
        [SerializeField, ReadOnly] private float _actionAFK;


        public List<CallBack> list = new List<CallBack>();
        
        private void OnEnable() {
            _inputAction = new MetromaActions();
            _inputAction.Gameplay.Enable();
            
            _inputAction.Gameplay.Move.performed += ctx => OnMove(ctx.ReadValue<Vector2>());
            _inputAction.Gameplay.Move.canceled += ctx => OnMove(Vector2.zero);
            
            _inputAction.Gameplay.Action.performed += ctx => OnAction(true);
            _inputAction.Gameplay.Action.canceled += ctx => OnAction(false);
        }
        private void OnDisable() {
            _inputAction.Gameplay.Disable();
        }

        private void FixedUpdate() {
            _moveAFK = _move != Vector2.zero ? 0.0f : _moveAFK + Time.fixedDeltaTime;
            _actionAFK = _action ? 0.0f : _actionAFK + Time.fixedDeltaTime;
        }
        
        
        private void OnMove(Vector2 move) {
            Vector2 last = _move;
            _move = move;
            
            // if (last == Vector2.zero && _move != Vector2.zero) OnMoveStart?.Invoke();
            // else if (last != Vector2.zero && _move == Vector2.zero) OnMoveEnd?.Invoke();
        }
        private void OnAction(bool action) {
            bool last = _action;
            _action = action;
            
            // if (last == false && _action == true) OnActionStart?.Invoke();
            // else if (last == true && _action == false) OnActionEnd?.Invoke();
        }
        
        public GameplayInputsData GetGameplayInputsData() => new GameplayInputsData(_move, _moveAFK, _action, _actionAFK);
    }
}
