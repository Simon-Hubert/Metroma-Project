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
        protected float accelerationLerp = 0.0f;
        protected float accelerationValue = 0.0f;
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
            
        }

        private void MoveStartLerp() {
            if (lerpCoroutine != null) StopCoroutine(lerpCoroutine);
            lerpCoroutine = StartCoroutine(MoveStartLerpCoroutine(accelerationTime, accelerationCurve));
        }
        private IEnumerator MoveStartLerpCoroutine(float duration, AnimationCurve curve) {
            if (duration == 0f)
            {
                accelerationValue = 1f;
                yield break;
            }
            
            
            while ()
            {
                
            }
            
            yield break;
        }
        
        private void MoveEndLerp() {
            if (lerpCoroutine != null) StopCoroutine(lerpCoroutine);
            lerpCoroutine = StartCoroutine(MoveEndLerpCoroutine(decelerationTime, decelerationCurve));
        }
        private IEnumerator MoveEndLerpCoroutine(float duration, AnimationCurve curve) {
            if (duration == 0f)
            {
                accelerationValue = 0f;
                yield break;
            }
            
            yield break;
        }
    }
}
