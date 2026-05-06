using UnityEngine;
using System.Collections.Generic;
using NaughtyAttributes;

namespace Metroma.Utils
{
    public class MoveLine : MonoBehaviour
    {
        [SerializeField] public List<Transform> points;

        public Vector3 GetStart { get => points.Count > 1 ? points[0].position : Vector3.zero; }
        public Vector3 GetEnd { get => points.Count > 1 ? points[points.Count - 1].position : Vector3.zero; }

        public Vector3 GetIndex(int index) => (points.Count > 1 && index >= 0 && index < points.Count) ? points[index].position : Vector3.zero;
        public int GetNbSegments { get => points.Count > 1 ? points.Count - 1 : 0; }

        [SerializeField, ReadOnly] private int _currentSegment;
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
        [SerializeField, ReadOnly] private int _passNext;
        public int GetPassNext { get => _passNext; }
        public void ResetPassNext() => _passNext = 0;

        public Vector3 GetSegmentStart(int index) => (index >= 0 && index < points.Count) ? points[index].position : Vector3.zero;
        public Vector3 GetSegmentStart() => GetSegmentStart(CurrentSegment);
        public Vector3 GetSegmentEnd(int index) => (index >= 0 && index < points.Count) ? points[index + 1].position : Vector3.zero;
        public Vector3 GetSegmentEnd() => GetSegmentEnd(CurrentSegment);
        public Vector3 GetSegmentNormal() => (GetSegmentEnd() - GetSegmentStart()).normalized;

        public bool IsAtStart(Vector3 target)
        {
            if (_currentSegment == 0)
            {
                Vector3 startToTarget = target - GetSegmentStart();
                Vector3 startToEnd = GetSegmentEnd() - GetSegmentStart();

                float segmentLength = startToEnd.magnitude;

                float dot = Vector3.Dot(startToTarget, GetSegmentNormal());
                float lerp = dot / segmentLength;

                if (lerp <= 0f) return true;
            }   
            
            return false;
        }
        public bool IsAtEnd(Vector3 target)
        {
            if (_currentSegment == GetNbSegments -1)
            {
                Vector3 startToTarget = target - GetSegmentStart();
                Vector3 startToEnd = GetSegmentEnd() - GetSegmentStart();

                float segmentLength = startToEnd.magnitude;

                float dot = Vector3.Dot(startToTarget, GetSegmentNormal());
                float lerp = dot / segmentLength;

                if (lerp >= 0f) return true;
            }   
            
            return false;
        }
        
        public Vector3 MoveOnLine(Vector3 target) {
            Vector3 startToTarget = target - GetSegmentStart();
            Vector3 startToEnd = GetSegmentEnd() - GetSegmentStart();

            float segmentLength = startToEnd.magnitude;

            float dot = Vector3.Dot(startToTarget, GetSegmentNormal());
            float lerp = dot / segmentLength;

            if (lerp < 0f) {
                Vector3 res = GetSegmentStart();
                CurrentSegment -= 1;
                return res;
            }
            else if (lerp > 1f) {
                Vector3 res = GetSegmentEnd();
                CurrentSegment += 1;
                return res;
            }
            else {
                _passNext = 0;
                return GetSegmentStart() + GetSegmentNormal() * dot;
            }
        }
    }
}
