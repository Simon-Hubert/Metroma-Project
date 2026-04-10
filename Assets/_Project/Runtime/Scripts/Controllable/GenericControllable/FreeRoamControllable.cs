using System;
using System.Collections;
using UnityEngine;
using NaughtyAttributes;
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

        [Header("Movement")]
        [SerializeField] protected bool constantSpeed = false;
        [SerializeField, Min(0)] protected float accelerationTime = 0.5f;
        [SerializeField]         protected AnimationCurve accelerationCurve;
        [SerializeField, Min(0)] protected float decelerationTime = 1.5f;
        [SerializeField]         protected AnimationCurve decelerationCurve;

        [SerializeField] protected float maxSpeed = 10.0f;

        [Header("Rotation")]
        [Tooltip("0 = no rotation speed")]
        [SerializeField, Min(0)] protected float rotationSpeed = 0f;
        [SerializeField, Range(0, 1)] protected float smoothRotation = 0.9f;
        [FormerlySerializedAs("aimDirection")] [SerializeField, ReadOnly] protected Vector2 moveDirection = Vector2.up;
        
        [Space(7)]
        [SerializeField, ReadOnly] float accelerationValue = 0.0f;
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
            if (!constantSpeed) {
                OnMoveStart += MoveStartLerp;
                OnMoveEnd += MoveEndLerp;
            }
        }
        protected void OnDisable() {
            if (!constantSpeed) {
                OnMoveStart -= MoveStartLerp;
                OnMoveEnd -= MoveEndLerp;
            }
        }

        protected void Start() {
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

            DirectionCheck();
            OrientationCheck();

            if (constantSpeed) {
                rb2D.linearVelocity = moveDirection * maxSpeed;
            }
            else {
                rb2D.linearVelocity = moveDirection * (accelerationValue * maxSpeed);
            }
        }
        
        #region Update Checks

        protected void DirectionCheck() {
            if (Inputs.move != Vector2.zero)
            {
                if (rotationSpeed <= 0) {
                    moveDirection = Inputs.move;
                }
                else
                {
                    float angleDisplace = Mathf.Clamp(Vector2.SignedAngle(moveDirection, Inputs.move),
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

        protected void OrientationCheck()
        {
            float angle = Vector2.SignedAngle(Vector2.up, moveDirection);
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        #endregion
        
        #region Move Methods
        private void MoveStartLerp() {
            if (Inputs.move == Vector2.zero) return;
            
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
