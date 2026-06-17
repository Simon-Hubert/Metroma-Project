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
        [SerializeField, Tooltip("Durée d'une seule secousse.")]
        private float ShakeDuration = 1f;
        
        [SerializeField, Tooltip("Force de la secousse.")]
        private float ShakeIntensity =  0.009f;
        
        [SerializeField, Tooltip("Fréquence du moteur (ex: 50 Hz).")]
        private float ShakeSpeed = 50f;
        
        [SerializeField, Tooltip("Nombre de répétitions (ex: 2 pour une notification). Si réglé sur 0, la vibration est infinie jusqu'à StopVibration() !")]
        private int RepeatCount = 3;
        
        [SerializeField, Tooltip("Pause entre chaque secousse (en secondes).")]
        private float PauseBetweenShakes = 0.5f;

        private Coroutine AnimationCoroutine;
        private Coroutine ShakeCoroutine;
        private bool bIsVisible = false;
        
        private Vector3 PreShakePosition;
        private Quaternion PreShakeRotation;
        
        public event Action OnPhoneShown;
        public event Action OnPhoneHidden;
        public event Action OnPhoneVibrated;


        private void Start()
        {
            if (PhoneTransform == null)
            {
                PhoneTransform = transform;
            }
            
            if (EnableMovement)
            {
                PhoneTransform.localPosition = HiddenLocalPosition;
            }
            
            bIsVisible = false;
        }


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


        [Button("Test Vibration")]
        public void VibratePhone()
        {
            if (ShakeCoroutine != null)
            {
                StopCoroutine(ShakeCoroutine);
                PhoneTransform.localPosition = PreShakePosition;
                PhoneTransform.localRotation = PreShakeRotation;
            }
            else
            {
                PreShakePosition = PhoneTransform.localPosition;
                PreShakeRotation = PhoneTransform.localRotation;
            }
                
            ShakeCoroutine = StartCoroutine(ShakeRoutine());
            
            OnPhoneVibrated?.Invoke();
        }


        public void StopVibration()
        {
            if (ShakeCoroutine != null)
            {
                StopCoroutine(ShakeCoroutine);
                ShakeCoroutine = null;

                if (EnableMovement)
                {
                    PhoneTransform.localPosition = bIsVisible ? VisibleLocalPosition : HiddenLocalPosition;
                }
                else
                {
                    PhoneTransform.localPosition = PreShakePosition;
                }
                
                PhoneTransform.localRotation = PreShakeRotation;
            }
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
            Vector3 BasePosition = PhoneTransform.localPosition;
            Quaternion BaseRotation = PhoneTransform.localRotation;
            
            int i = 0;
            while (RepeatCount <= 0 || i < RepeatCount)
            {
                float Elapsed = 0f;
                float NextShakeUpdate = 0f;
                Vector3 TargetOffset = Vector3.zero;
                float TargetRotOffset = 0f;
                
                while (Elapsed < ShakeDuration)
                {
                    Elapsed += Time.unscaledDeltaTime;
                    
                    if (Elapsed >= NextShakeUpdate)
                    {
                        NextShakeUpdate = Elapsed + (1f / ShakeSpeed);
                        
                        TargetOffset = new Vector3(
                            UnityEngine.Random.Range(-ShakeIntensity, ShakeIntensity),
                            UnityEngine.Random.Range(-ShakeIntensity * 0.2f, ShakeIntensity * 0.2f),
                            0f
                        );
                        
                        TargetRotOffset = UnityEngine.Random.Range(-ShakeIntensity * 20f, ShakeIntensity * 20f);
                    }
                    
                    PhoneTransform.localPosition = BasePosition + TargetOffset;
                    PhoneTransform.localRotation = BaseRotation * Quaternion.Euler(0f, 0f, TargetRotOffset);
                    
                    yield return null;
                }
                
                PhoneTransform.localPosition = BasePosition;
                PhoneTransform.localRotation = BaseRotation;
                
                i++;
                if (RepeatCount > 0 && i >= RepeatCount)
                {
                    break;
                }
                
                yield return new WaitForSecondsRealtime(PauseBetweenShakes);
            }

            if (EnableMovement)
            {
                PhoneTransform.localPosition = bIsVisible ? VisibleLocalPosition : HiddenLocalPosition;
            }
            else
            {
                PhoneTransform.localPosition = BasePosition;
            }
            PhoneTransform.localRotation = BaseRotation;
        }
    }
}
