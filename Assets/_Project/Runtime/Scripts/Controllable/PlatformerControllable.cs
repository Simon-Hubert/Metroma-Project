using System;
using System.Collections;
using NaughtyAttributes;
using UnityEngine;

namespace Metroma
{
    public class PlatformerControllable : Controllable
    {
        [Header("Essentials")]
        [SerializeField] protected Rigidbody rb2D;
        
        [Header("Movement")]
        [SerializeField, Min(0)] protected float accelerationTime = 0.5f;
        [SerializeField] protected AnimationCurve accelerationCurve;
        [SerializeField, Min(0)] protected float decelerationTime = 1.5f;
        [SerializeField] protected AnimationCurve decelerationCurve;
        [SerializeField, Min(0)] protected float returnTime = 0.5f;
        [SerializeField] protected AnimationCurve returnCurve;
        protected float accelerationValue = 0.0f;
        protected float directionValue = 0.0f; // 1.0f = go right
        protected Coroutine lerpCoroutine;
        [Space(7)]
        [SerializeField] protected float maxSpeed = 100.0f;
        
        [Header("Ground Detection")]
        [SerializeField] protected Vector2 groundDetectionOffset = Vector2.zero;
        [SerializeField, Min(0)] protected float groundDetectionSize = 1.0f;
        [SerializeField, ReadOnly] protected bool isGrounded = false;


        protected void OnEnable()
        {
            OnMoveStart += MoveStartLerp;
            OnMoveEnd += MoveEndLerp;
        }
        protected void OnDisable() {
            OnMoveStart -= MoveStartLerp;
            OnMoveEnd -= MoveEndLerp;
        }

        protected void FixedUpdate() {
            
            
            rb2D.AddForce(Vector3.right * (directionValue * accelerationValue * maxSpeed), ForceMode.Acceleration);
        }

        private void DirectionCheck()
        {
            if (accelerationValue == 0.0f) {
                directionValue = 0.0f;
            }
            else if ((Inputs.move.x != 0 && directionValue == 0) || Mathf.Sign(Inputs.move.x) == Mathf.Sign(directionValue)) {
                if (Inputs.move.x < 0) directionValue = -1.0f;
                else directionValue = 1.0f;
            }
            else if (Mathf.Sign(Inputs.move.x) != Mathf.Sign(directionValue)) {
                //TODO
            }
        }

        private void MoveStartLerp() {
            if (lerpCoroutine != null) StopCoroutine(lerpCoroutine);
            lerpCoroutine = StartCoroutine(MoveStartLerpCoroutine(accelerationTime, accelerationCurve));
        }
        private IEnumerator MoveStartLerpCoroutine(float duration, AnimationCurve curve) {
            if (duration > 0f || accelerationValue < 1f)
            {
                float offset = accelerationValue;
                float ratio = 1 - offset;

                float lerp = 0.0f;
                while (lerp < 1.0f)
                {
                    yield return new WaitForFixedUpdate();
                
                    lerp += Time.fixedDeltaTime / (duration * ratio);
                    accelerationValue = curve.Evaluate(offset + (lerp * ratio));
                }
            }
            
            accelerationValue = 1f;
            yield break;
        }
        
        private void MoveEndLerp() {
            if (lerpCoroutine != null) StopCoroutine(lerpCoroutine);
            lerpCoroutine = StartCoroutine(MoveEndLerpCoroutine(decelerationTime, decelerationCurve));
        }
        private IEnumerator MoveEndLerpCoroutine(float duration, AnimationCurve curve) {
            if (duration > 0f || accelerationValue > 0f)
            {
                float ratio = accelerationValue;

                float lerp = 1.0f;
                while (lerp > 0.0f)
                {
                    yield return new WaitForFixedUpdate();
                
                    lerp -= Time.fixedDeltaTime / (duration * ratio);
                    accelerationValue = curve.Evaluate(lerp * ratio);
                }
            }
            
            accelerationValue = 0f;
            yield break;
        }
    }
}
