using System;
using System.Collections.Generic;
using NaughtyAttributes;
using TMPro;
using UnityEngine;
using Metroma.Utils;

namespace Metroma
{
    [Serializable]
    public enum TrolleyState
    {
        None = 0,
        Start = 1,
        End = 2,
        Action = 3,
        Intro = 4,
        Finishing = 5
    }
    
    [Serializable]
    public struct Dilemma
    {
        public TrolleyChoiceSO choiceA;
        public TrolleyChoiceSO choiceB;
    }
    
    public class TrolleyLogic : MonoBehaviour
    {
        [SerializeField] private TrolleyControllable _ctrl;
        [SerializeField] private List<Dilemma> _dilemmas;
        [SerializeField, ReadOnly] private int _dilemmaIndex = 0;
        [SerializeField, ReadOnly] private TrolleyState _trolleyState;
        
        private Direction _ctrlDir = Direction.NONE;
        private void ChangeDirection(Direction dir) => _ctrlDir = dir;
        

        [Header("Timer")]
        [SerializeField, Min(0)] private float _actionTimer;
        [SerializeField] private TextMeshProUGUI _timerDisplay;
        private void UpdateTimer(float time) => _timerDisplay.text = TimerDisplayValue(GetFalseTime(time), _showSecondsOnly);
        [SerializeField, Min(0)] private float _falseTimer;
        public float GetFalseTime(float time) => _falseTimer == 0 || _actionTimer == 0 ? 0 : time * (_falseTimer / _actionTimer);
        [SerializeField] private bool _showSecondsOnly;

        [Header("Choices")]

        [SerializeField] private RopeLogic _choiceL;
        [SerializeField] private RopeLogic _choiceR;
        [Space(7)]
        [SerializeField] private Transform _choicesSpawnOrigin;
        [SerializeField, HideInInspector] private float _originHeight;
        [SerializeField] private float _gapBetweenChoices = 5;
        [SerializeField] private float _spawnHeight = 5;
        [SerializeField] private float _spawnTime = 1;
        [SerializeField] private float _endOfCourseHeight = -10;
        public float GetCourseLerp(float time) => _actionTimer == 0 ? 1f : Mathf.Clamp01(time / _actionTimer);

        private void OnValidate()
        {
            if (_choicesSpawnOrigin)
            {
                _originHeight = _choicesSpawnOrigin.localPosition.y;
                
                if (_choiceL != null) _choiceL.transform.position = _choicesSpawnOrigin.position + Vector3.left * (_gapBetweenChoices / 2);
                if (_choiceR != null) _choiceR.transform.position = _choicesSpawnOrigin.position + Vector3.right * (_gapBetweenChoices / 2);
            }
        }

        private void Start() {
            if (_timerDisplay) _timerDisplay.text = TimerDisplayValue(0, _showSecondsOnly);

            _trolleyState = TrolleyState.None;
            if (_ctrl) _ctrl.OnChooseDirection += ChangeDirection;
            
            _choicesSpawnOrigin.localPosition = new Vector3(_choicesSpawnOrigin.localPosition.x, _originHeight + _spawnHeight, _choicesSpawnOrigin.localPosition.z);
        }

        private void OnDisable()
        {
            if (_ctrl) _ctrl.OnChooseDirection -= ChangeDirection;
        }

#if UNITY_EDITOR
        [Button]
        private void Editor_TestTimerDisplay()
        {
            Debug.Log("Test Timer Display : \n" +
                      $"123 D : {TimerDisplayValue(123f, false)}\n" +
                      $"123 S : {TimerDisplayValue(123f, true)}\n" +
                      $"123.4567 D : {TimerDisplayValue(123.4567f, false)}\n" +
                      $"123.4567 S : {TimerDisplayValue(123.4567f, true)}\n" +
                      $"1.2345 D : {TimerDisplayValue(1.2345f, false)}\n" +
                      $"1.2345 S : {TimerDisplayValue(1.2345f, true)}\n" +
                      $"0.12345 D : {TimerDisplayValue(0.12345f, false)}\n" +
                      $"0.12345 S : {TimerDisplayValue(0.12345f, true)}\n");
        }
        #endif
        private string TimerDisplayValue(float time, bool onlySec) {
            if (time <= 0f) return onlySec ? "00" : "00.00";
            
            int secRounded = (int)time;
            int sec = onlySec && time > secRounded ? secRounded + 1 : secRounded;
            string result = sec.ToString("D2");
                
            if (!onlySec)
            {
                int dec = (int)((time - secRounded) * 100f);
                result = $"{result}.{dec.ToString("D2")}";
            }

            return result;
        }

        [Button]
        public void PlayDilemmaList() {
            if (!_choiceL || !_choiceR || !_choicesSpawnOrigin) {
                Debug.LogError($"{name}'s Trolley Logic : Missing core element to start Dilemmas, pls check this Script Choices or ChoiceSpawnOrigin", this);
                return;
            }

            _ = PlayDilemmas();
        }

        public async Awaitable PlayDilemmas() {
            try {
                for (_dilemmaIndex = 0; _dilemmaIndex < _dilemmas.Count; _dilemmaIndex++) {
                    Debug.Log($"{name}'s Trolley Logic : Dilemma {_dilemmaIndex}", this);

                    _choiceL.choice.SetTrolleySO = _dilemmas[_dilemmaIndex].choiceA;
                    _choiceR.choice.SetTrolleySO = _dilemmas[_dilemmaIndex].choiceB;

                    await DilemmaStartSequence();
                    await DilemmaActionSequence();
                    await DilemmaEndSequence();
                }
            }
            catch (Exception e) {
                Debug.LogException(e);
            }
        }

        private async Awaitable DilemmaStartSequence() {
            Debug.Log($"{name}'s Trolley Logic : Start Dilemma", this);

            try {
                Vector3 pos = _choicesSpawnOrigin.localPosition;

                _choicesSpawnOrigin.localPosition = new Vector3(pos.x, _originHeight + _spawnHeight, pos.z);
                _choiceL.ResetRopePosition();
                _choiceR.ResetRopePosition();
                UpdateTimer(_actionTimer);
                
                float time = _spawnTime;
                while (time > 0f) {
                    time -= Time.deltaTime;

                    _choicesSpawnOrigin.localPosition = new Vector3(pos.x,
                        _originHeight + (_spawnHeight * (time / _spawnTime)), pos.z);

                    _choiceL.ResetRopePosition();
                    _choiceR.ResetRopePosition();

                    await Awaitable.NextFrameAsync();
                }
                
                _choicesSpawnOrigin.localPosition = new Vector3(pos.x, _originHeight, pos.z);
                _choiceL.ResetRopePosition();
                _choiceR.ResetRopePosition();
            }
            catch (Exception e) {
                Debug.LogException(e);
            }
        }

        private async Awaitable DilemmaActionSequence() {
            Debug.Log($"{name}'s Trolley Logic : Action Dilemma", this);

            try {
                _ctrlDir = Direction.NONE;

                float time = _actionTimer;
                while (_ctrlDir == Direction.NONE) {
                    if (_actionTimer > 0) {
                        UpdateTimer(time);
                        
                        time -= Time.deltaTime;
                        if (time <= 0f) _ctrlDir = Direction.RIGHT | Direction.LEFT;
                    }
                    
                    await Awaitable.NextFrameAsync();
                }
                
                if ((_ctrlDir & Direction.LEFT) != 0) _choiceL.CutRopeAtPosition(_ctrl.transform.position);
                if ((_ctrlDir & Direction.RIGHT) != 0) _choiceR.CutRopeAtPosition(_ctrl.transform.position);

                await Awaitable.WaitForSecondsAsync(3);
            }
            catch (Exception e) {
                Debug.LogException(e);
            }
        }
        
        private async Awaitable DilemmaEndSequence() {
            Debug.Log($"{name}'s Trolley Logic : End Dilemma", this);

            try {
                Vector3 pos = _choicesSpawnOrigin.localPosition;
                
                _choicesSpawnOrigin.localPosition = new Vector3(pos.x, _originHeight, pos.z);
                
                float time = _spawnTime;
                while  (time > 0f) {
                    time -= Time.deltaTime;
                    
                    _choicesSpawnOrigin.localPosition = new Vector3(pos.x, _originHeight + (_spawnHeight * (1 - (time/_spawnTime))) , pos.z);
                    
                    await Awaitable.NextFrameAsync();
                }
                
                _choicesSpawnOrigin.localPosition = new Vector3(pos.x, _originHeight + _spawnHeight, pos.z);
                
                _choiceL.ResetRopePosition();
                _choiceR.ResetRopePosition();
            }
            catch (Exception e) {
                Debug.LogException(e);
            }
        }

        private void MoveRopes(float delta, Vector3 start, Vector3 end) {
            
        }
    }
}
