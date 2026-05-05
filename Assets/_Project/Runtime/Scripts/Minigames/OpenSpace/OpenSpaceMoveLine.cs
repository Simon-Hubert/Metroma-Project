using System;
using System.Collections.Generic;
using UnityEngine;

namespace Metroma
{
    [Serializable]
    public struct MoveLine
    {
        [SerializeField] public List<Transform> points;

        public Vector3 GetStart { get => points.Count > 1 ? points[0].position : Vector3.zero; }
        public Vector3 GetEnd { get => points.Count > 1 ? points[points.Count - 1].position : Vector3.zero; }

        public Vector3 GetIndex(int index) => (points.Count > 1 && index >= 0 && index < points.Count) ? points[index].position : Vector3.zero;
        public int GetNbSegments { get => points.Count > 1 ? points.Count - 1 : 0; }

        private int _currentSegment;
        public int CurrentSegment {
            get => _currentSegment;
            set {
                if (value >= GetNbSegments)
                {
                    _passNext = 1;
                }
                else if (value < 0)
                {
                    _passNext = -1;
                }
                else
                {
                    _currentSegment = value;
                    _passNext = 0;
                }
            }
        }
        private int _passNext;
        public int GetPassNext { get => _passNext; }

        public Vector3 GetSegmentStart(int index) => (index >= 0 && index < points.Count) ? points[index].position : Vector3.zero;
        public Vector3 GetSegmentStart() => GetSegmentStart(CurrentSegment);
        public Vector3 GetSegmentEnd(int index) => (index >= 0 && index < points.Count) ? points[index + 1].position : Vector3.zero;
        public Vector3 GetSegmentEnd() => GetSegmentEnd(CurrentSegment);
        public Vector3 GetSegmentNormal() => (GetSegmentEnd() - GetSegmentStart()).normalized;

        
        public Vector3 MoveOnLine(Vector3 target) {
            Vector3 fromStart = target - GetSegmentStart();
            Vector3 StartToEnd = GetSegmentEnd() - GetSegmentStart();

            Vector3 projection = Vector3.Project(fromStart, StartToEnd.normalized);
            float lerp = projection.magnitude / StartToEnd.magnitude;

            if (projection != Vector3.zero && StartToEnd.normalized != projection.normalized) {
                Vector3 res = GetSegmentStart();
                _currentSegment--;
                return res;
            }
            else if (lerp > 1) {
                Vector3 res = GetSegmentEnd();
                _currentSegment++;
                return res;
            }
            else {
                return projection;
            }
        }
    }
    
    public class OpenSpaceMoveLine : MonoBehaviour
    {
        [SerializeField] private OpenSpaceControllable _ctrl;
        
        [Space(7)] [SerializeField] private int _lineIndex = 0;
        [SerializeField] private List<MoveLine> _moveLines = new List<MoveLine>();

        private void FixedUpdate()
        {
            Vector3 displace = (_ctrl.GetSpeed * Time.fixedDeltaTime) * _moveLines[_lineIndex].GetSegmentNormal();
            
            _ctrl.transform.position = _moveLines[_lineIndex].MoveOnLine(_ctrl.transform.position + displace);

            _lineIndex = Mathf.Clamp(_lineIndex + _moveLines[_lineIndex].GetPassNext, 0, _moveLines.Count - 1);
        }
        
        private void OnDrawGizmos() {
            for (int i = 0; i < _moveLines.Count; i++) {
                Gizmos.color = i % 2 == 0 ? Color.cyan : Color.red;

                for (int j = 0; j < _moveLines[i].GetNbSegments; j++) {
                    Gizmos.DrawLine(_moveLines[i].GetSegmentStart(j), _moveLines[i].GetSegmentEnd(j));
                }
            }
        }
    }
}
