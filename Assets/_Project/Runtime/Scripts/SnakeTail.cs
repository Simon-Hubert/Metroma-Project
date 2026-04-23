using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Shapes;

namespace Metroma
{
    public class SnakeTail : ImmediateModeShapeDrawer
    {
        [SerializeField, Min(0.01f)] private float _sectionLenght = 0.2f;
        [SerializeField, Min(1)] private int _sectionNumber = 50;
        [SerializeField] private float thick = 1f;
        [SerializeField] private AnimationCurve curvy;
        [SerializeField] private float serpentThick = 1f;
        [SerializeField] private AnimationCurve serpenty;

        private struct Point
        {
            public Vector3 Position;
            public Vector2 Normal;
        }

        private Vector2 _lastPos;
        private Point[] _parts;
        private int _iterator = 0;

        private void OnValidate() {
            _parts = new Point[_sectionNumber];
            for (int i = 0; i < _sectionNumber; i++) {
                _parts[i].Position = transform.position;
            }
            _iterator = 0;
        }

        private void Start() {
            
            _parts = new Point[_sectionNumber];
            for (int i = 0; i < _sectionNumber; i++) {
                _parts[i].Position = transform.position;
            }
            _iterator = 0;
            
            _lastPos = transform.position;
            StartCoroutine(UpdateParts());
        }

        IEnumerator UpdateParts() {
            while (true) {
                if (Vector2.Distance(_lastPos, transform.position) >= _sectionLenght) {
                    Vector2 dir = _lastPos - (Vector2)transform.position;
                    _lastPos = transform.position;
                    _parts[_iterator].Position = transform.position;
                    _parts[_iterator].Normal = new Vector2(-dir.y, dir.x).normalized;
                    _iterator = Iterate(_iterator);
                }
                yield return new WaitForEndOfFrame();
            }
        }

        public override void DrawShapes(Camera cam) {
            using (Draw.Command(cam)) {
                PolylinePath path = new PolylinePath();
                PolylinePath path2 = new PolylinePath();

                foreach (int point in GetPoints()) {
                    float p = (float)ArrayDistance(point, _iterator) / _sectionNumber;
                    float dist = Vector3.Distance(_parts[point].Position, transform.position);
                    Vector3 pos = _parts[point].Position + (Vector3)_parts[point].Normal * Mathf.Sin(0.2f * 2*Mathf.PI * _parts[point].Position.x) * Mathf.Sin(0.2f * 2*Mathf.PI * (_parts[point].Position.y+1)) * serpentThick * serpenty.Evaluate(p);
                    path.AddPoint(pos, curvy.Evaluate(p) * thick);
                    path2.AddPoint(pos, curvy.Evaluate(p) * thick * 0.15f, Color.red);
                }

                Draw.Polyline(path, false, 1f, PolylineJoins.Round);
                Draw.Polyline(path2, false, 1f, PolylineJoins.Round);
            }
        }

        private int Iterate(int i) {
            return (i + 1 >= _sectionNumber ? 0 : i + 1);
        }

        private int ArrayDistance(int from, int to) {
            to--;
            if (from > to) return (_sectionNumber - from) + to;
            else return to - from;
        }

        IEnumerable<int> GetPoints()
        {
            for (int i = 0; i < _sectionNumber; i++)
            {
                yield return ArrayDistance(i, _iterator);
            }
        }
    }
}
