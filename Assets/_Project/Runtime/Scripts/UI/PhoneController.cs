using System;
using System.Collections;
using UnityEngine;
using NaughtyAttributes;


namespace Metroma.UI
{

    public class PhoneController : MonoBehaviour
    {
        [SerializeField] private Transform PhoneTransform;

        [Header("Animation Settings")]
        [SerializeField, Tooltip("Désactiver pour figer le téléphone (utile si un Animator gère déjà la position)")] 
        private bool EnableMovement = true;
        
        [ShowIf("EnableMovement")]
        [SerializeField] private Vector3 HiddenLocalPosition;
        
        [ShowIf("EnableMovement")]
        [SerializeField] private Vector3 VisibleLocalPosition;
        
        [ShowIf("EnableMovement")]
        [SerializeField] private float AnimationDuration = 0.5f;
        
        [ShowIf("EnableMovement")]
        [SerializeField] private AnimationCurve AnimationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Vibration Settings")]
        [SerializeField] private float ShakeDuration = 0.4f;
        [SerializeField] private float ShakeIntensity = 0.05f;
        [SerializeField] private float ShakeSpeed = 50f;

        private Coroutine AnimationCoroutine;
        private Coroutine ShakeCoroutine;
        private bool bIsVisible = false;
        
        public event Action OnPhoneShown;
        public event Action OnPhoneHidden;
        public event Action OnPhoneVibrated;


        private void Start()
        {
            if (PhoneTransform == null)
            {
                PhoneTransform = transform;
            }
            
            // Set initial state
            if (EnableMovement)
            {
                PhoneTransform.localPosition = HiddenLocalPosition;
            }
            
            bIsVisible = false;
        }


        /// <summary>
        /// Fait apparaître le téléphone à l'écran.
        /// </summary>
        public void ShowPhone()
        {
            if (bIsVisible)
                return;

            bIsVisible = true;

            if (EnableMovement)
            {
                if (AnimationCoroutine != null)
                    StopCoroutine(AnimationCoroutine);

                AnimationCoroutine = StartCoroutine(AnimatePhone(VisibleLocalPosition));
            }
            
            OnPhoneShown?.Invoke();
        }


        /// <summary>
        /// Range le téléphone.
        /// </summary>
        public void HidePhone()
        {
            if (!bIsVisible)
                return;

            bIsVisible = false;

            if (EnableMovement)
            {
                if (AnimationCoroutine != null)
                    StopCoroutine(AnimationCoroutine);

                AnimationCoroutine = StartCoroutine(AnimatePhone(HiddenLocalPosition));
            }
            
            UIManager.Instance.CloseAllPanels();
            
            OnPhoneHidden?.Invoke();
        }


        /// <summary>
        /// Déclenche un effet de tremblement visuel du téléphone et un événement.
        /// </summary>
        public void VibratePhone()
        {
            if (ShakeCoroutine != null)
                StopCoroutine(ShakeCoroutine);
                
            ShakeCoroutine = StartCoroutine(ShakeRoutine());
            
            OnPhoneVibrated?.Invoke();
        }


        private IEnumerator AnimatePhone(Vector3 TargetPosition)
        {
            Vector3 StartPosition = PhoneTransform.localPosition;
            float Elapsed = 0f;

            while (Elapsed < AnimationDuration)
            {
                Elapsed += Time.unscaledDeltaTime;
                float t = Elapsed / AnimationDuration;
                float CurveEval = AnimationCurve.Evaluate(t);
                
                PhoneTransform.localPosition = Vector3.LerpUnclamped(StartPosition, TargetPosition, CurveEval);
                yield return null;
            }

            PhoneTransform.localPosition = TargetPosition;
        }


        private IEnumerator ShakeRoutine()
        {
            float Elapsed = 0f;
            Vector3 BasePosition = PhoneTransform.localPosition;

            while (Elapsed < ShakeDuration)
            {
                Elapsed += Time.unscaledDeltaTime;
                
                float OffsetX = Mathf.Sin(Elapsed * ShakeSpeed) * ShakeIntensity;
                float OffsetY = Mathf.Cos(Elapsed * ShakeSpeed * 1.2f) * ShakeIntensity;
                
                PhoneTransform.localPosition = BasePosition + new Vector3(OffsetX, OffsetY, 0f);
                
                yield return null;
            }

            if (EnableMovement)
            {
                PhoneTransform.localPosition = bIsVisible ? VisibleLocalPosition : HiddenLocalPosition;
            }
            else
            {
                PhoneTransform.localPosition = BasePosition;
            }
        }
    }
}
