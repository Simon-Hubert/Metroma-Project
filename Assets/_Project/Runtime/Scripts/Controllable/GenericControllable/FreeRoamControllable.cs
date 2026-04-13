using System;
using System.Collections;
using UnityEngine;
using NaughtyAttributes;
using NUnit.Framework;
using UnityEngine.Serialization;

namespace Metroma
{
    public class FreeRoamControllable : Controllable
    {
        protected enum MoveState
        {
            NONE = 0,
            ACCELERATING = 1,
            DECELERATING = 2
        }
        
        [Header("Essentials")]
        [SerializeField] protected Rigidbody2D rb2D;

        #region Movement
        [Header("Movement")]
        [SerializeField] public bool constantSpeed = false;
        [SerializeField, Min(0)] protected float accelerationTime = 0.5f;
        [SerializeField]         protected AnimationCurve accelerationCurve;
        [SerializeField, Min(0)] protected float decelerationTime = 1f;
        [SerializeField]         protected AnimationCurve decelerationCurve;
        [Space(7)]
        [SerializeField] protected bool keepLastInputAsDirection;
        private Vector2 lastInputMove;
        [SerializeField] protected float maxSpeed = 10.0f;
        #endregion
        
        #region Rotation
        [Header("Rotation")]
        [Tooltip("Placebo pour Ferdinand, qu'il pleure pas")]
        [SerializeField] private bool TêteQuiSerpente;
        [Tooltip("0 = no rotation speed")]
        [SerializeField, Min(0)] protected float rotationSpeed = 0f;
        [SerializeField, UnityEngine.Range(0, 1)] protected float smoothRotation = 0.9f;
        [SerializeField, ReadOnly] protected Vector2 moveDirection = Vector2.up;
        public Vector2 GetDirection { get => moveDirection; }
        #endregion

        [Space(7)]
        [SerializeField] protected bool rotateControllable = true;
        [SerializeField, ReadOnly] protected float accelerationValue = 0.0f;
        protected Coroutine lerpCoroutine;
        [SerializeField, ReadOnly] protected MoveState moveState = MoveState.NONE;
        
        protected void OnValidate() {
            if (!rb2D) {
                if (TryGetComponent<Rigidbody2D>(out rb2D)) {
                    Debug.LogError($"{name} : Missing RigidBody2D");
                }
            }
        }
        
        protected void OnEnable() {
            OnMoveStart += MoveStartLerp;
            OnMoveEnd += MoveEndLerp;
        }
        protected void OnDisable() {
            OnMoveStart -= MoveStartLerp;
            OnMoveEnd -= MoveEndLerp;

            if (rb2D) {
                rb2D.linearVelocity = Vector2.zero;
            }
        }

        protected override void Start() {
            base.Start();
            
            // Error proof
            if (!rb2D) {
                if (TryGetComponent<Rigidbody2D>(out rb2D)) {
                    Debug.LogError($"{name} : Missing RigidBody2D");
                }
            }

            moveDirection = Vector2.up;
        }

        protected override void FixedUpdate() {
            base.FixedUpdate();
            
            // Error Proof
            if (!rb2D) {
                Debug.LogError($"{name} : Missing RigidBody2D");
                return;
            }

            DesactiveCheck();
            
            DirectionCheck();
            if (rotateControllable) OrientationCheck();
            
            rb2D.linearVelocity = moveDirection * (accelerationValue * maxSpeed);
        }
        
        #region Update Checks

        protected void DirectionCheck() {
            if (Inputs.move != Vector2.zero || keepLastInputAsDirection)
            {
                if (Inputs.move != Vector2.zero)
                    lastInputMove = Inputs.move;
                
                if (rotationSpeed <= 0) {
                    moveDirection = lastInputMove;
                }
                else
                {
                    float angleDisplace = Mathf.Clamp(Vector2.SignedAngle(moveDirection, lastInputMove),
                                              -rotationSpeed * Time.fixedDeltaTime,
                                              rotationSpeed * Time.fixedDeltaTime)
                                          * smoothRotation ;
                    
                    float angleCurrent = Vector2.SignedAngle(Vector2.up, moveDirection);
                    
                    moveDirection = new Vector2(
                        - 1 * Mathf.Sin(Mathf.Deg2Rad * (angleDisplace + angleCurrent)),
                        1 * Mathf.Cos(Mathf.Deg2Rad * (angleDisplace + angleCurrent))
                        );
                }
            }
        }

        protected void OrientationCheck() {
            float angle = Vector2.SignedAngle(Vector2.up, moveDirection);
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        protected void DesactiveCheck() {
            if (!IsActive && moveState != MoveState.DECELERATING && accelerationValue != 0) {
                if (lerpCoroutine != null) StopCoroutine(lerpCoroutine);
                lerpCoroutine = StartCoroutine(MoveDecelerationLerpCoroutine(decelerationTime, decelerationCurve));
            }
        }
        
        #endregion
        
        #region Move Methods
        private void MoveStartLerp() {
            if (Inputs.move == Vector2.zero && !constantSpeed) return;
            
            if (lerpCoroutine != null) StopCoroutine(lerpCoroutine);
            lerpCoroutine = StartCoroutine(MoveAccelerationLerpCoroutine(accelerationTime, accelerationCurve));
        }
        private IEnumerator MoveAccelerationLerpCoroutine(float duration, AnimationCurve curve) {
            moveState = MoveState.ACCELERATING;
            
            if (duration > 0f && accelerationValue < 1f) {
                float offset = accelerationValue;
                float ratio = 1 - offset;

                float lerp = 0.0f;
                while (lerp < 1.0f) {
                    yield return new WaitForFixedUpdate();
                
                    lerp += Time.fixedDeltaTime / (duration * ratio);
                    accelerationValue = curve.Evaluate(offset + (lerp * ratio));
                }
            }
            
            moveState = MoveState.NONE;
            accelerationValue = 1f;
            yield break;
        }
        private void MoveEndLerp() {
            if (constantSpeed) return;
            
            if (lerpCoroutine != null) StopCoroutine(lerpCoroutine);
            lerpCoroutine = StartCoroutine(MoveDecelerationLerpCoroutine(decelerationTime, decelerationCurve));
        }
        private IEnumerator MoveDecelerationLerpCoroutine(float duration, AnimationCurve curve) {
            moveState = MoveState.DECELERATING;
            
            if (duration > 0f && accelerationValue > 0f) {
                float ratio = accelerationValue;

                float lerp = 1.0f;
                while (lerp > 0.0f) {
                    yield return new WaitForFixedUpdate();
                
                    lerp -= Time.fixedDeltaTime / (duration * ratio);
                    accelerationValue = curve.Evaluate(lerp * ratio);
                }
            }
            
            moveState = MoveState.NONE;
            accelerationValue = 0f;
            yield break;
        }
        #endregion
    }
}
