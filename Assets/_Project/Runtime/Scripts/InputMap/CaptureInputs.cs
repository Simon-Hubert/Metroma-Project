using System;
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
    
    public class CaptureInputs : MonoBehaviour
    {
        private MetromaActions _inputAction;

        private Vector2 _move;
        private float _moveAFK;
        private bool _action;
        private float _actionAFK;

        private void OnAwake()
        {
            _inputAction = new MetromaActions();
        }
        
        private void OnEnable()
        {
            _inputAction.Gameplay.Enable();
            
            _inputAction.Gameplay.Move.performed += ctx => _move = ctx.ReadValue<Vector2>();
            _inputAction.Gameplay.Move.canceled += ctx => _move = Vector2.zero;
            
            _inputAction.Gameplay.Action.performed += ctx => _action = ctx.ReadValue<bool>();
            _inputAction.Gameplay.Action.canceled += ctx => _action = false;
        }
        private void OnDisable()
        {
            _inputAction.Gameplay.Disable();
        }

        private void FixedUpdate()
        {
            _moveAFK = _move != Vector2.zero ? 0.0f : _moveAFK + Time.fixedDeltaTime;
            _actionAFK = _action ? 0.0f : _actionAFK + Time.fixedDeltaTime;
        }

        public GameplayInputsData GetGameplayInputsData() => new GameplayInputsData(_move, _moveAFK, _action, _actionAFK);
    }
}
