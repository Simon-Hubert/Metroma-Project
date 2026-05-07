using System;
using System.Collections.Generic;
using NaughtyAttributes;
using TMPro;
using UnityEngine;
using System.Threading;

namespace Metroma
{
    [Serializable]
    public struct Dilemma
    {
        public TrolleyChoiceSO choiceA;
        public TrolleyChoiceSO choiceB;
    }
    
    public class TrolleyLogic : MonoBehaviour
    {
        [SerializeField] private List<Dilemma> _dilemmas;
        [SerializeField, ReadOnly] private int _dilemmaIndex;
        

        [Header("Timer")]
        [SerializeField, Min(0)] private float _timer;
        private float _currentTimer;
        [SerializeField] private TextMeshProUGUI _timerDisplay;
        [SerializeField] private float _falseTimer;
        public float GetCurrentFalseTimer { get => _falseTimer == 0 || _timer == 0 ? 0 : _currentTimer * (_falseTimer / _timer); }

        [Header("Folders")]
        [SerializeField] private Transform _folderSpawnOrigin;
        [SerializeField] private float _gapBetweenFolders;
        [SerializeField] private float _endOfCourseHeight = -10;
        public float GetFolderCourse { get => _timer == 0 ? 1f : Mathf.Clamp01(_currentTimer / _timer); }

        private void Start() {
            _timerDisplay.text = "00:00";
        }

        private void DisplayOnTimer(float time, bool onlySec) {
            if (onlySec) {
                
            }
        }

        [Button]
        public void PlayDilemmaList() {
            
        }

        public async Awaitable PlayDilemma() {
            
        }
    }
}
