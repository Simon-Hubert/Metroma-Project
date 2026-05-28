using System;
using System.Collections;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;
using Metroma.Utils;
using NaughtyAttributes;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Metroma
{
    public class OpenSpaceLogic : MonoBehaviour
    {
        [SerializeField] private OpenSpaceControllable _ctrl;
        [SerializeField] private SpriteRenderer _ctrlSprite;
        [Space(7)]
        [SerializeField, ReadOnly] private bool _isLoopActive;
        [Button]
        public void ActiveLoop() => ActiveLoop(!_isLoopActive);
        public void ActiveLoop(bool isActive) {
            _isLoopActive = isActive;
            if (_isLoopActive) {
                _trackIndex = 0;
                _ctrl.transform.position = _moveTracks[_trackIndex].GetStart;
            }
            Debug.Log($"OpenSpaceLogic : Change Active State to {_isLoopActive}");
        }
        
        [Header("Tracks")]
        [SerializeField, ReadOnly] private int _trackIndex = 0;

        public int TrackIndex {
            get => _trackIndex;
            private set {
                if (_moveTracks == null) return;
                
                _trackIndex = Mathf.Clamp(value, 0, _moveTracks.Count - 1);
                if (_ctrlSprite != null) _ctrlSprite.sortingOrder = _moveTracks[_trackIndex].GetOrderInLayer;
            }
        }
        [SerializeField] private bool _noGoBack = false;
        [SerializeField] private List<MoveLine> _moveTracks = new List<MoveLine>();
        
        [ConditionParam] public bool GetIsAtEnd { get => GetCurrentTrackState >= GetTrackStateEnd; }
        [ConditionParam] public int GetTrackStateEnd { get => _moveTracks.Count; }
        [ConditionParam] public int GetCurrentTrackState { get => _trackIndex + _moveTracks[_trackIndex].GetPassNext; }

        [Header("Projection")]
        [SerializeField, ReadOnly] private bool _inProjection = false;
        [SerializeField, ReadOnly] private bool _recovering = false;
        [SerializeField, Min(0.1f)] private float _projectionSpeed = 30f;
        [SerializeField, Min(0)] private float _recoveryTime = 1f;
        
        [Header("Signals")]
        [SerializeField] private bool _projectedIfOnTrack = false;
        public bool GetProjectedIfOnTrack { get => _projectedIfOnTrack; }
        [SerializeField] private bool _callAllSignals = false;
        [Space(7)]
        [SerializeField] private List<AOPSignal> _signals;
        [SerializeField, ReadOnly] private List<AOPSignal> _signalsAlwaysCalled;
        [SerializeField] private Dictionary<int, List<AOPSignal>> _signalsByTrack = new Dictionary<int, List<AOPSignal>>();
        private bool _signalPlaying;
        
        [Header("Delays")]
        [SerializeField, Min(0), Tooltip("Time Between each alert")] private Vector2 _activateDelay;
        [SerializeField, Tooltip("Will use the X value only")] private bool _useDelayDelta = true;
        private float _activateCurrentTime = 0f;
        [Space(7)]
        [SerializeField, Min(0), Tooltip("Time between the starting alert and the projection window")] private Vector2 _alertingTime;
        [SerializeField, Tooltip("Will use the X value only")] private bool _useAlertingDelta = false;
        [Space(7)]
        [SerializeField, Min(0), Tooltip("Time before the projection actual window")] private float _reactionTime = 0f;
        [Space(7)]
        [SerializeField, Min(0), Tooltip("Duration of the projection window")] private Vector2 _activeTime;
        [SerializeField, Tooltip("Will use the X value only")] private bool _useActiveDelta = false;

        [Header("Tempo - GP Validation")]
        [SerializeField] private ConditionalEvent _condition;
        
        private void OnValidate() {
            if (_activateDelay.y < _activateDelay.x) _activateDelay.y = _activateDelay.x;
            if (_alertingTime.y < _alertingTime.x) _alertingTime.y = _alertingTime.x;
            if (_activeTime.y < _activeTime.x) _activeTime.y = _activeTime.x;
        }

        private void Start() {
            SortSignals();
        }
        [Button]
        private void SortSignals() {
            _signalsByTrack.Clear();
            _signalsAlwaysCalled.Clear();
            TrackIndex = 0;
            
            foreach (AOPSignal signal in _signals) {
                if (signal != null) {
                    if (signal.IsAlwaysCalled()) {
                        _signalsAlwaysCalled.Add(signal);
                    }
                    else {
                        if (!_signalsByTrack.ContainsKey(signal.GetTrackId()))
                            _signalsByTrack.Add(signal.GetTrackId(), new List<AOPSignal>());
                        
                        _signalsByTrack[signal.GetTrackId()].Add(signal);
                    }
                }
            }
        }
        
        private void FixedUpdate() {
            if (!_isLoopActive) return;
            
            if (!_inProjection) {
                Vector3 displace = (_ctrl.GetSpeed * Time.fixedDeltaTime) * _moveTracks[TrackIndex].GetSegmentNormal();
                _ctrl.transform.position = _moveTracks[TrackIndex].MoveOnLine(_ctrl.transform.position + displace);

                CheckLineIndex();
                CheckSignals();
            }
            else {
                Vector3 displace = (-_projectionSpeed * Time.fixedDeltaTime) * _moveTracks[TrackIndex].GetSegmentNormal();
                _ctrl.transform.position = _moveTracks[TrackIndex].MoveOnLine(_ctrl.transform.position + displace);
                
                if (!_recovering && _moveTracks[TrackIndex].IsAtStart(_ctrl.transform.position)) {
                    _recovering = true;
                    StartCoroutine(ProjectionRecovery(_recoveryTime));
                }
            }
            
            if (GetIsAtEnd) _condition.Evaluate();
        }
        
        private void CheckLineIndex() {
            int prevIndex = TrackIndex;
            
            int indexIncrement = _moveTracks[TrackIndex].GetPassNext;
            if ((!_noGoBack && indexIncrement != 0) || (_noGoBack && indexIncrement > 0)) {
                int lastIndex = TrackIndex;
                TrackIndex = Mathf.Clamp(TrackIndex + indexIncrement, 0, _moveTracks.Count - 1);
                if (TrackIndex != lastIndex) _moveTracks[TrackIndex].ResetPassNext();
                
                if (prevIndex != TrackIndex )
                    _ctrl.transform.position = indexIncrement < 0 ? _moveTracks[TrackIndex].GetEnd : _moveTracks[TrackIndex].GetStart;
            }
        }

        private void CheckSignals() {
            // Si le timer doit descendre et qu'aucun signal se joue
            if (!_signalPlaying && _activateCurrentTime > 0f) {
                _activateCurrentTime -= Time.fixedDeltaTime;

                // Si le timer atteint 0, appel de signal
                if (_activateCurrentTime <= 0f) {
                    AOPSignal choosen = null;
                    AOPSignal[] signalToCall = Array.Empty<AOPSignal>();

                    if (_callAllSignals) {
                        signalToCall = _signals.ToArray();
                    }
                    else {
                        // Si un signal est associé à la Track actuel
                        if (_signalsByTrack.ContainsKey(TrackIndex)) {
                            List<AOPSignal> selection = new List<AOPSignal>();

                            foreach (AOPSignal signal in _signalsByTrack[TrackIndex]) {
                                if (signal.GetSegmentId() == _moveTracks[TrackIndex].CurrentSegment) selection.Add(signal);
                            }

                            if (selection.Count > 0) choosen = selection[Random.Range(0, selection.Count)];
                            else choosen = _signalsByTrack[TrackIndex][Random.Range(0, _signalsByTrack[TrackIndex].Count)];
                        }
                        // Sinon, si il y a un signal, prendre aléatoirement
                        else if (choosen == null && _signals.Count > 0) choosen = _signals[Random.Range(0, _signals.Count)];
                        
                        
                        // S'il y a un signal, l'appeler
                        if (choosen != null) {
                            signalToCall = new AOPSignal[_signalsAlwaysCalled.Count + 1];
                            Array.Copy(_signalsAlwaysCalled.ToArray(), signalToCall, _signalsAlwaysCalled.Count);
                            signalToCall[^1] = choosen;
                        }
                        else {
                            signalToCall = _signalsAlwaysCalled.ToArray();
                        }
                    }
                    
                    float alert = _useAlertingDelta ? Random.Range(_alertingTime.x, _alertingTime.y) : _alertingTime.x;;
                    float active = _useActiveDelta ? Random.Range(_activeTime.x, _activeTime.y) : _activeTime.x;

                    if (signalToCall.Length > 0)
                    {
                        foreach (AOPSignal signal in signalToCall) {
                            if (signal != null) signal.ActiveSignal(this, alert, _reactionTime, active);
                        }
                        _signalPlaying = true;
                        
                    }
                }
            }
            // Si le timer doit se reset parce qu'il a atteint 0 et qu'acun signal se joue
            else if (!_signalPlaying && _activateCurrentTime <= 0f) {
                _activateCurrentTime = _useDelayDelta ? Random.Range(_activateDelay.x, _activateDelay.y) : _activateDelay.x;
            }
            
        }

        private IEnumerator ProjectionRecovery(float duration) {
            while (duration > 0) {
                yield return new WaitForFixedUpdate();
                duration -= Time.fixedDeltaTime;
            }
            
            _inProjection = false;
            _recovering = false;
            _moveTracks[_trackIndex].ResetPassNext();
        }
        
        [Button]
        public void CallProjection() { 
            _inProjection = true;
            _activateCurrentTime = 0;
        }
        public void CallProjection(int track) {
            if (!GetProjectedIfOnTrack || TrackIndex == track) {
                CallProjection();
            }
        }
        public void CallSignalEnded() { 
            _signalPlaying = false;
            _activateCurrentTime = 0;
        } 

        public bool IsCtrlMoving() => _ctrl.GetSpeed != 0;
        
        private void OnDrawGizmos() {
            for (int i = 0; i < _moveTracks.Count; i++) {
                if (_moveTracks[i] != null) {
                    switch (i % 4) {
                        case 0 : Gizmos.color = Color.red; break;
                        case 1 : Gizmos.color = Color.orange; break;
                        case 2 : Gizmos.color = Color.yellow; break;
                        case 3 : Gizmos.color = Color.greenYellow; break;
                    }

                    for (int j = 0; j < _moveTracks[i].GetNbSegments; j++) {
                        Gizmos.DrawLine(_moveTracks[i].GetSegmentStart(j), _moveTracks[i].GetSegmentEnd(j));
                    }
                }
            }
        }
    }
}
