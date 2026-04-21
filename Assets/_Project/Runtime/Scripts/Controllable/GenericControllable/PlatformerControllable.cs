using System;
using System.Collections;
using NaughtyAttributes;
using UnityEngine;

namespace Metroma
{
    public class PlatformerControllable : Controllable
    {
        [Serializable]
        protected enum MoveState
        {
            NONE = 0,
            ACCELERATING = 1,
            DECELERATING = 2,
            TURNING_AROUND = 3
        }
        public enum JumpType 
        {
            AtActionStart = 0,
            AtActionEnd = 1,
            AtActionDynamic = 2
        }
        
        [Header("Essentials")]
        [SerializeField] protected Rigidbody2D rb2D;
        
        #region Movement
        [Header("Movement")]
        [SerializeField, Min(0)] protected float accelerationTime = 0.5f;
        [SerializeField]         protected AnimationCurve accelerationCurve;
        [SerializeField, Min(0)] protected float decelerationTime = 1.5f;
        [SerializeField]         protected AnimationCurve decelerationCurve;
        [Space(7)]
        [SerializeField, Min(0)] protected float turnAroundStartTime = 0.25f;
        [SerializeField]         protected AnimationCurve turnAroundStartCurve;
        [SerializeField, Min(0)] protected float turnAroundEndTime = 0.5f;
        [SerializeField]         protected AnimationCurve turnAroundEndCurve;
        
        [SerializeField, ReadOnly] float accelerationValue = 0.0f;
        protected Coroutine lerpCoroutine;
        [SerializeField, ReadOnly] protected MoveState moveState = MoveState.NONE;
        #endregion
        
        protected int currentDir = 0;
        protected float dirLerp = 0.0f; // 1.0f = go right // -1.0f = go left //
        protected int GetInputDir {
            get {
                if (Inputs.move.x != 0.0f) return (int)Mathf.Sign(Inputs.move.x);
                else return 0;
            }
        }
        private float lastXPosition = 0;
        
        [Space(7)]
        [SerializeField] protected float maxSpeed = 100.0f;

        [SerializeField] private float stoppedTolerance = 0.01f;
        
        #region Ground Detection
        [Header("Ground Detection")]
        [SerializeField] protected Vector2 groundDetectionOffset = Vector2.zero;
        [SerializeField, Min(0)] protected float groundDetectionSize = 1.0f;
        
        [SerializeField] protected float isntGroundedMoveFactor = 0.5f;
        [SerializeField, Min(0)] protected float coyoteTime = 0.2f;
        [Space(7)]
        [SerializeField, ReadOnly] protected bool isGrounded = false;
        private Coroutine coyoteTimeCoroutine;
        #endregion
        
        #region Jump
        [Header("Jump")]
        [SerializeField] protected bool canJump = true;
        [Tooltip("Set before starting the game")]
        [SerializeField] protected JumpType jumpType = JumpType.AtActionStart;
        [SerializeField, Min(0)] protected float dynamicJumpDelay = 0.3f;
        private float currentDynamicDelay = 0.0f;
        [Space(7)]
        [SerializeField] protected float jumpingForce = 10.0f;
        [SerializeField] protected float delayBeforeJump = 0f;
        [SerializeField] protected float delayAfterJump = 0.5f;
        [Space(7)]
        [SerializeField, ReadOnly] protected bool isJumping = false;
        protected Coroutine jumpCoroutine;
        protected Coroutine dynamicJumpCoroutine;
        #endregion

        protected void OnValidate() {
            if (!rb2D) {
                if (TryGetComponent<Rigidbody2D>(out rb2D)) {
                    Debug.LogError($"{name} : Missing RigidBody2D");
                }
            }
        }

        protected override void OnEnable() {
            base.OnEnable();
        }
        protected override void OnDisable() {
            base.OnDisable();
        }

        protected override void Start() {
            base.Start();
            
            // Error proof
            if (!rb2D) {
                if (TryGetComponent<Rigidbody2D>(out rb2D)) {
                    Debug.LogError($"{name} : Missing RigidBody2D");
                }
            }
            
            lastXPosition = transform.position.x;
        }
        
        protected override void FixedUpdate() {
            base.FixedUpdate();

            // Error proof
            if (!rb2D) {
                Debug.LogError($"{name} : Missing RigidBody2D");
                return;
            }
            
            MoveStateCheck();
            GroundCheck();
            
            rb2D.linearVelocity = (Vector2.up * rb2D.linearVelocity.y) + Vector2.right * (currentDir * accelerationValue * maxSpeed);
            
            WallCheck();
        }

        #region Update Checks
        private void MoveStateCheck() {
            if (GetInputDir == 0 && (moveState == MoveState.ACCELERATING || (moveState == MoveState.NONE && accelerationValue != 0.0f))) {
                MoveEndLerp();
            }
            
            if (accelerationValue == 0.0f && currentDir != GetInputDir && GetInputDir != 0) {
                currentDir *= -1;
                
                if (lerpCoroutine != null) StopCoroutine(lerpCoroutine);
                lerpCoroutine = StartCoroutine(MoveAccelerationLerpCoroutine(turnAroundEndTime, turnAroundEndCurve));
            }
            else if (moveState == MoveState.NONE && accelerationValue == 0.0f && currentDir != 0) currentDir = 0;
            
            if (accelerationValue != 0.0f && currentDir != GetInputDir && GetInputDir != 0 && currentDir != 0 && moveState != MoveState.TURNING_AROUND) {
                moveState = MoveState.TURNING_AROUND;
                if (lerpCoroutine != null) StopCoroutine(lerpCoroutine);
                lerpCoroutine = StartCoroutine(MoveDecelerationLerpCoroutine(turnAroundStartTime, turnAroundStartCurve));
            }
            else if (accelerationValue != 0.0f && (currentDir == GetInputDir || currentDir == 0) && GetInputDir != 0 && moveState != MoveState.ACCELERATING) {
                if (lerpCoroutine != null) StopCoroutine(lerpCoroutine);
                lerpCoroutine = StartCoroutine(MoveAccelerationLerpCoroutine(accelerationTime, accelerationCurve));
            }
        }
        private void WallCheck() {
            if ((GetInputDir == 0 || GetInputDir != currentDir) && 
                lastXPosition + stoppedTolerance >= transform.position.x && lastXPosition - stoppedTolerance <= transform.position.x) {
                accelerationValue = 0.0f;
                if (lerpCoroutine != null) StopCoroutine(lerpCoroutine);
            }

            lastXPosition = transform.position.x;
        }
        private void GroundCheck()
        {
            RaycastHit2D hitL = Physics2D.Raycast((Vector2)transform.position + groundDetectionOffset * new Vector2(1 * (transform.lossyScale.x / 2), 1), Vector2.down, groundDetectionSize);
            RaycastHit2D hitR = Physics2D.Raycast((Vector2)transform.position + groundDetectionOffset * new Vector2(-1 * (transform.lossyScale.x / 2), 1), Vector2.down, groundDetectionSize);

#if UNITY_EDITOR
            Debug.DrawLine(
                (Vector2)transform.position + groundDetectionOffset * new Vector2(1 * (transform.lossyScale.x / 2), 1), 
                (Vector2)transform.position + groundDetectionOffset * new Vector2(1 * (transform.lossyScale.x / 2), 1) + Vector2.down * groundDetectionSize,
                Color.red, 2f);
            Debug.DrawLine(
                (Vector2)transform.position + groundDetectionOffset * new Vector2(-1 * (transform.lossyScale.x / 2), 1),
                (Vector2)transform.position + groundDetectionOffset * new Vector2(-1 * (transform.lossyScale.x / 2), 1) + Vector2.down * groundDetectionSize,
                Color.red, 2f);
#endif
            
            if (hitL || hitR) {
                if (showDebugLog) Debug.Log("grounded");
                isGrounded = true;
                if (coyoteTimeCoroutine != null) {
                    StopCoroutine(coyoteTimeCoroutine);
                    coyoteTimeCoroutine = null;
                }
            }
            else {
                if (isGrounded && coyoteTimeCoroutine == null)
                {
                    if (showDebugLog) Debug.Log("coyote");
                    coyoteTimeCoroutine = StartCoroutine(CoyoteCoroutine());
                }
            }
        }
        private IEnumerator CoyoteCoroutine()
        {
            if (coyoteTime != 0f) yield return new WaitForSeconds(coyoteTime);
            
            isGrounded = false;
            yield break;
        }
        #endregion

        #region Move Methods
        private void MoveStartLerp() {
            if (Inputs.move.x == 0.0f) return;
            
            if (currentDir == 0) currentDir = GetInputDir;
            
            if (lerpCoroutine != null) StopCoroutine(lerpCoroutine);
            lerpCoroutine = StartCoroutine(MoveAccelerationLerpCoroutine(accelerationTime, accelerationCurve));
        }
        private IEnumerator MoveAccelerationLerpCoroutine(float duration, AnimationCurve curve) {
            moveState = MoveState.ACCELERATING;
            
            if (duration > 0f && accelerationValue < 1f) {
                // Debug.Log("Start Accelerating");
                
                float offset = accelerationValue;
                float ratio = 1 - offset;

                float lerp = 0.0f;
                while (lerp < 1.0f) {
                    yield return new WaitForFixedUpdate();
                
                    lerp += Time.fixedDeltaTime / (duration * ratio) * (isGrounded ? 1.0f : isntGroundedMoveFactor);
                    accelerationValue = curve.Evaluate(offset + (lerp * ratio));
                }
                
                // Debug.Log("Stop Accelerating");
            }
            
            moveState = MoveState.NONE;
            accelerationValue = 1f;
            yield break;
        }
        
        private void MoveEndLerp() {
            if (currentDir == 0) currentDir = GetInputDir;
            
            if (lerpCoroutine != null) StopCoroutine(lerpCoroutine);
            lerpCoroutine = StartCoroutine(MoveDecelerationLerpCoroutine(decelerationTime, decelerationCurve));
        }
        private IEnumerator MoveDecelerationLerpCoroutine(float duration, AnimationCurve curve) {
            if (moveState != MoveState.TURNING_AROUND) moveState = MoveState.DECELERATING;

            
            if (duration > 0f && accelerationValue > 0f) {
                // Debug.Log("Start Decelerating");
                
                float ratio = accelerationValue;

                float lerp = 1.0f;
                while (lerp > 0.0f) {
                    yield return new WaitForFixedUpdate();
                
                    lerp -= Time.fixedDeltaTime / (duration * ratio) * (isGrounded ? 1.0f : isntGroundedMoveFactor);
                    accelerationValue = curve.Evaluate(lerp * ratio);
                }
                
                // Debug.Log("Stop Decelerating");
            }
            
            moveState = MoveState.NONE;
            accelerationValue = 0f;
            
            yield break;
        }
        #endregion

        #region Jump Methods
        protected void DynamicJump()
        {
            if (dynamicJumpCoroutine != null) {
                StopCoroutine(dynamicJumpCoroutine);
                dynamicJumpCoroutine = null;
            }
            dynamicJumpCoroutine = StartCoroutine(DynamicCoroutine());
        }
        private IEnumerator DynamicCoroutine() {
            currentDynamicDelay = 0f;
            while (currentDynamicDelay < dynamicJumpDelay) {
                currentDynamicDelay += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
            currentDynamicDelay = dynamicJumpDelay;
            
            Jumping();
            yield break;
        }
        private void Jumping() {
            if (!canJump ||
                !isGrounded || isJumping) return;
            
            switch (jumpType)
            {
                case JumpType.AtActionDynamic:
                    jumpCoroutine = StartCoroutine(JumpCoroutine(jumpingForce * (currentDynamicDelay / dynamicJumpDelay)));
                    break;
                default:
                    jumpCoroutine = StartCoroutine(JumpCoroutine(jumpingForce));
                    break;
            }
        }
        private IEnumerator JumpCoroutine(float jumpForce) {
            if (delayBeforeJump > 0f) {
                yield return new WaitForSeconds(delayBeforeJump);

                if (!isGrounded || isJumping) yield break;
            }
            
            isJumping = true;
            rb2D.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

            float secureTimer = 1.0f;
            while (isGrounded && secureTimer > 0) {
                yield return new WaitForFixedUpdate();
                secureTimer -= Time.fixedDeltaTime;
            }
            while (!isGrounded || coyoteTimeCoroutine != null) {
                yield return new WaitForFixedUpdate();
            }
            
            yield return new WaitForSeconds(delayAfterJump);
            isJumping = false;
            yield break;
        }
        #endregion
        
        #region Inputs Events

        protected override void InputMoveStart(Vector2 _move) {
            base.InputMoveStart(_move);
            
            MoveStartLerp();
        }
        protected override void InputMoveEnd(Vector2 _move) {
            base.InputMoveEnd(_move);
            
            MoveEndLerp();
        }

        protected override void InputActionStart(bool _action) {
            base.InputActionStart(_action);
            
            switch (jumpType) {
                case JumpType.AtActionDynamic:
                    DynamicJump();
                    break;
                case JumpType.AtActionStart:
                    Jumping();
                    break;
            }
        }
        protected override void InputActionEnd(bool _action) {
            base.InputActionEnd(_action);
            
            switch (jumpType) {
                case JumpType.AtActionEnd :
                    Jumping();
                    break;
                case JumpType.AtActionDynamic:
                    Jumping();
                    break;
            }
        }
        
        #endregion
    } 
}
