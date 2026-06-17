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
    
    public enum QTEResult
    {
        None = -1,
        Succeeded = 1,
        Failed = 0
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

        [Tooltip("Optional: Transform to use as an anchor for the QTE UI. " +
        "If null, the QTE will be anchored to the Controllable's transform.")]
        public Transform _uiAnchorPosition;

        public QTEStep(QTEInputType expectedInput, float validationTime, float tolerance, Transform transform)
        {
            this.ExpectedInput = expectedInput;
            this.ValidationTime = validationTime;
            this.Tolerance = tolerance;
            this._uiAnchorPosition = transform;
        }

        public float WindowStart => Mathf.Max(0f, ValidationTime - Tolerance); //Zone de validation
        public float WindowEnd => ValidationTime + Tolerance;
    }
    
    public class QTEControllable : AControllable
    {
        [Header("QTE Sequence")]
        [Tooltip("Ordered list of inputs that make up the QTE")]
        [SerializeField] private List<QTEStep> _steps = new List<QTEStep>();
        [Button, ContextMenu("CopyFirstAnchorPositionToAllSteps")]
        private void CopyFirstAnchorPositionToAllSteps() 
        {
            if(_steps.Count == 0 || _steps[0]._uiAnchorPosition == null) return;
            for(int i = 1; i < _steps.Count; i++)
            {
                QTEStep qteStep = _steps[i];
                qteStep._uiAnchorPosition = _steps[0]._uiAnchorPosition;
            }
        }
        [Button, ContextMenu("ApplyDefaultAnchorPositionToAllSteps")]
        private void ApplyDefaultAnchorPositionToAllSteps() 
        {
            for(int i = 0; i < _steps.Count; i++)
            {
                QTEStep qteStep = _steps[i];
                qteStep._uiAnchorPosition = transform;
            }
        }

        [Header("Settings")]
        [Tooltip("Automatically start the QTE when the Controllable becomes active.")]
        [SerializeField] private bool _autoStart = false;
        [Tooltip("If true, pressing the wrong input during a step fails the QTE. " +
                 "If false, wrong inputs are ignored.")]
        [SerializeField] private bool _failOnWrongInput = true;
        [Tooltip("If true, pressing the right input before the window opens fails the QTE. " +
                 "If false, an early press is ignored (the player can press again later).")]
        [SerializeField] private bool _failOnEarlyInput = true;
        
        [Header("UI")]
        [SerializeField] private QTEReticle _reticlePrefab;
        [Tooltip("Optional parent for the spawned reticles. Defaults to this transform.")]
        [SerializeField] private Transform _reticleParent;

        private QTEReticle _activeReticle;

        //Debug
        [Header("Runtime (read only)")]
        [SerializeField, ReadOnly] private bool _isRunning;
        [ConditionParam, SerializeField, ReadOnly] private int _currentIndex = -1;
        [SerializeField, ReadOnly] private float _stepElapsed;
        [SerializeField, ReadOnly] private bool _bumped;
        [SerializeField, ReadOnly] private QTEResult _result = QTEResult.None;

        [Foldout("Events")] public UnityEvent<int> OnStepValidated;
        [Foldout("Events")] public UnityEvent<int> OnStepFailed;
        [Foldout("Events")] public UnityEvent OnQTECompleted;
        [Foldout("Events")] public UnityEvent OnQTEFailed;
        
        public bool IsRunning => _isRunning;
        public int CurrentIndex => _currentIndex;
        [ConditionParam] public int StepCount => _steps.Count;
        [ConditionParam] public int Result => (int)_result;
        public QTEResult ResultState => _result;
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
        public void AddStep(QTEInputType input, float validationTime, float tolerance, Transform anchor)
        {
            _steps.Add(new QTEStep(input, validationTime, tolerance, anchor));
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
            _result = QTEResult.None;

            if (showDebugLog) Debug.Log($"QTE {name} : started ({_steps.Count} steps).");

            EnterStep();
        }

        [Button]
        public void StopQTE()
        {
            _isRunning = false;
            _currentIndex = -1;
            _stepElapsed = 0f;
            if (_activeReticle != null) _activeReticle.Hide();
            _activeReticle = null;
        }

        private void EnterStep()
        {
            _stepElapsed = 0f;
            _bumped = false;
            _activeReticle = null;

            if (_reticlePrefab == null) return;

            Transform anchor = CurrentStep._uiAnchorPosition != null ? CurrentStep._uiAnchorPosition : transform;
            Transform parent = _reticleParent != null ? _reticleParent : transform;

            _activeReticle = Instantiate(_reticlePrefab, anchor.position, Quaternion.identity, parent);
            _activeReticle.Show(anchor.position);
            _activeReticle.SetApproach(0f);
        }

        protected override void Update()
        {
            base.Update();
            if (!_isRunning || !IsActive) return;

            _stepElapsed += Time.deltaTime;

            float validation = Mathf.Max(0.0001f, CurrentStep.ValidationTime);
            if (_activeReticle != null) _activeReticle.SetApproach(_stepElapsed / validation);

            if (!_bumped && _stepElapsed >= CurrentStep.ValidationTime)
            {
                _bumped = true;
                if (_activeReticle != null) _activeReticle.Bump();
            }

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
            if (_activeReticle != null) _activeReticle.PlaySuccess();

            _currentIndex++;

            if (_currentIndex >= _steps.Count) Complete();
            else EnterStep();
        }

        private void FailStep(string reason)
        {
            if (showDebugLog)
                Debug.Log($"QTE {name} : step {_currentIndex} ({CurrentStep.ExpectedInput}) failed -> {reason}.");

            OnStepFailed?.Invoke(_currentIndex);
            if (_activeReticle != null) _activeReticle.PlayFail();
            Fail();
        }

        private void Complete()
        {
            _isRunning = false;
            _result = QTEResult.Succeeded;
            if (showDebugLog) Debug.Log($"QTE {name} : completed.");
            OnQTECompleted?.Invoke();
            _currentIndex = -1;
        }

        private void Fail()
        {
            _isRunning = false;
            _result = QTEResult.Failed;
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
