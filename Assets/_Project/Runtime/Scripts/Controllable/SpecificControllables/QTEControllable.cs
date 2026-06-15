using System;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;

namespace Metroma
{

    public enum QTEInputType //type de qte
    {
        Action = 0,
        MoveUp = 1,
        MoveDown = 2,
        MoveLeft = 3,
        MoveRight = 4
    }
    
    [Serializable]
    public struct QTEStep //Un qte
    {
        [Tooltip("Input the player has to perform for this step.")]
        public QTEInputType ExpectedInput;

        [Tooltip("Target time (seconds) at which the input is expected. " +
                 "Ex: 2 -> the player should press the input around 2s after the step started.")]
        [Min(0f)] public float ValidationTime;

        [Tooltip("Accepted delta (seconds) around the validation time. " +
                 "Valid window = [validationTime - tolerance ; validationTime + tolerance].")]
        [Min(0f)] public float Tolerance;

        public QTEStep(QTEInputType expectedInput, float validationTime, float tolerance)
        {
            this.ExpectedInput = expectedInput;
            this.ValidationTime = validationTime;
            this.Tolerance = tolerance;
        }

        public float WindowStart => Mathf.Max(0f, ValidationTime - Tolerance); //Zone de validation
        public float WindowEnd => ValidationTime + Tolerance;
    }
    
    public class QTEControllable : AControllable
    {
        [Header("QTE Sequence")]
        [Tooltip("Ordered list of inputs that make up the QTE")]
        [SerializeField] private List<QTEStep> _steps = new List<QTEStep>();

        [Header("Settings")]
        [Tooltip("Automatically start the QTE when the Controllable becomes active.")]
        [SerializeField] private bool _autoStart = false;
        [Tooltip("If true, pressing the wrong input during a step fails the QTE. " +
                 "If false, wrong inputs are ignored.")]
        [SerializeField] private bool _failOnWrongInput = true;
        [Tooltip("If true, pressing the right input before the window opens fails the QTE. " +
                 "If false, an early press is ignored (the player can press again later).")]
        [SerializeField] private bool _failOnEarlyInput = true;

        //Debug
        [Header("Runtime (read only)")]
        [SerializeField, ReadOnly] private bool _isRunning;
        [SerializeField, ReadOnly] private int _currentIndex = -1;
        [SerializeField, ReadOnly] private float _stepElapsed;

        [Foldout("Events")] public UnityEvent<int> OnStepValidated;
        [Foldout("Events")] public UnityEvent<int> OnStepFailed;
        [Foldout("Events")] public UnityEvent OnQTECompleted;
        [Foldout("Events")] public UnityEvent OnQTEFailed;
        
        public bool IsRunning => _isRunning;
        public int CurrentIndex => _currentIndex;
        public int StepCount => _steps.Count;
        public QTEStep CurrentStep => _steps[_currentIndex];

        public float CurrentStepProgress
        {
            get
            {
                if (!_isRunning || _currentIndex < 0) return 0f;
                float end = CurrentStep.WindowEnd;
                return end > 0f ? Mathf.Clamp01(_stepElapsed / end) : 0f;
            }
        }

        #region Sequence building
        public void AddStep(QTEInputType input, float validationTime, float tolerance)
        {
            _steps.Add(new QTEStep(input, validationTime, tolerance));
        }

        public void AddStep(QTEStep step) => _steps.Add(step);

        public void ClearSteps() => _steps.Clear();
        #endregion

        #region QTE lifecycle
        [Button]
        public void StartQTE()
        {
            if (_steps == null || _steps.Count == 0)
            {
                Debug.LogWarning($"QTE {name} : no step to play.");
                return;
            }

            _isRunning = true;
            _currentIndex = 0;
            _stepElapsed = 0f;

            if (showDebugLog) Debug.Log($"QTE {name} : started ({_steps.Count} steps).");
        }

        [Button]
        public void StopQTE()
        {
            _isRunning = false;
            _currentIndex = -1;
            _stepElapsed = 0f;
        }

        protected override void Update()
        {
            base.Update();
            if (!_isRunning || !IsActive) return;

            _stepElapsed += Time.deltaTime;

            if (_stepElapsed > CurrentStep.WindowEnd)
            {
                FailStep("timeout");
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (_autoStart && IsActive) StartQTE();
        }

        public override bool IsActive
        {
            get => base.IsActive;
            set
            {
                base.IsActive = value;
                if (value && _autoStart && !_isRunning) StartQTE();
            }
        }
        #endregion

        #region Input callbacks
        protected override void InputActionStart(bool action)
        {
            base.InputActionStart(action);
            EvaluateInput(QTEInputType.Action);
        }

        protected override void InputMoveStart(Vector2 move)
        {
            base.InputMoveStart(move);
            EvaluateInput(MoveToInput(move));
        }
        
        private void EvaluateInput(QTEInputType pressed)
        {
            if (!_isRunning || _currentIndex < 0) return;

            QTEStep step = CurrentStep;

            if (pressed != step.ExpectedInput)
            {
                if (_failOnWrongInput) FailStep($"wrong input ({pressed} instead of {step.ExpectedInput})");
                return;
            }

            if (_stepElapsed < step.WindowStart)
            {
                if (_failOnEarlyInput) FailStep($"too early ({_stepElapsed:0.00}s < {step.WindowStart:0.00}s)");
                return;
            }

            ValidateStep();
        }
        #endregion

        #region Validation
        private void ValidateStep()
        {
            if (showDebugLog)
                Debug.Log($"QTE {name} : step {_currentIndex} ({CurrentStep.ExpectedInput}) validated at {_stepElapsed:0.00}s.");

            OnStepValidated?.Invoke(_currentIndex);

            _currentIndex++;
            _stepElapsed = 0f;

            if (_currentIndex >= _steps.Count) Complete();
        }

        private void FailStep(string reason)
        {
            if (showDebugLog)
                Debug.Log($"QTE {name} : step {_currentIndex} ({CurrentStep.ExpectedInput}) failed -> {reason}.");

            OnStepFailed?.Invoke(_currentIndex);
            Fail();
        }

        private void Complete()
        {
            _isRunning = false;
            _currentIndex = -1;
            if (showDebugLog) Debug.Log($"QTE {name} : completed.");
            OnQTECompleted?.Invoke();
        }

        private void Fail()
        {
            _isRunning = false;
            _currentIndex = -1;
            OnQTEFailed?.Invoke();
        }
        #endregion

        //arrondi la dire pour un qte joystick (jsp si ça marche)
        private static QTEInputType MoveToInput(Vector2 move)
        {
            if (Mathf.Abs(move.x) >= Mathf.Abs(move.y))
                return move.x >= 0f ? QTEInputType.MoveRight : QTEInputType.MoveLeft;
            return move.y >= 0f ? QTEInputType.MoveUp : QTEInputType.MoveDown;
        }
    }
}
