using System;
using System.Collections.Generic;
using UnityEngine;
using Metroma.Utils;
using NaughtyAttributes;

namespace Metroma
{
    public class OpenSpaceLogic : MonoBehaviour
    {
        [SerializeField] private OpenSpaceControllable _ctrl;
        
        [Space(7)] [SerializeField] private int _lineIndex = 0;
        [SerializeField] private bool _noGoBack = false;
        [SerializeField] private List<MoveLine> _moveTracks = new List<MoveLine>();

        [Space(7)]
        [SerializeField, ReadOnly] private bool _inProjection = false;
        [SerializeField] private float _projectionSpeed = -30f;

        private void FixedUpdate() {
            if (!_inProjection) {
                Vector3 displace = (_ctrl.GetSpeed * Time.fixedDeltaTime) * _moveTracks[_lineIndex].GetSegmentNormal();
                _ctrl.transform.position = _moveTracks[_lineIndex].MoveOnLine(_ctrl.transform.position + displace);

                CheckLineIndex();
            }
            else {
                Vector3 displace = (_projectionSpeed * Time.fixedDeltaTime) * _moveTracks[_lineIndex].GetSegmentNormal();
                _ctrl.transform.position = _moveTracks[_lineIndex].MoveOnLine(_ctrl.transform.position + displace);
                
                if (_moveTracks[_lineIndex].IsAtStart(_ctrl.transform.position))
                {
                    _inProjection = false;
                    _moveTracks[_lineIndex].ResetPassNext();
                }
            }
        }
        private void CheckLineIndex()
        {
            int prevIndex = _lineIndex;
            
            int indexIncrement = _moveTracks[_lineIndex].GetPassNext;
            if ((!_noGoBack && indexIncrement != 0) || (_noGoBack && indexIncrement > 0))
            {
                _lineIndex = Mathf.Clamp(_lineIndex + indexIncrement, 0, _moveTracks.Count - 1);
                Debug.Log($"LINE INDEX - index : {prevIndex} -> {_lineIndex}");
                _moveTracks[_lineIndex].ResetPassNext();
                
                if (prevIndex != _lineIndex )
                    _ctrl.transform.position = indexIncrement < 0 ? _moveTracks[_lineIndex].GetEnd : _moveTracks[_lineIndex].GetStart;
            }
        }
        
        [Button]
        public void Projection() => _inProjection = true;
        
        private void OnDrawGizmos() {
            for (int i = 0; i < _moveTracks.Count; i++)
            {
                if (_moveTracks[i] != null)
                {
                    switch (i % 4)
                    {
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
