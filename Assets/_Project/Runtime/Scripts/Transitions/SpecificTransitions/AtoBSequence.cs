using System;
using NaughtyAttributes;
using UnityEngine;

namespace Metroma
{
    public class AtoBSequence : ASequencable
    {
        [SerializeField] private Transform _objectToMove;
        [SerializeField] private Transform _A;
        [SerializeField] private Transform _B;
        [SerializeField] private float _duration;
        [SerializeField] private bool _useCurve;
        [SerializeField, ShowIf("_useCurve")] private AnimationCurve _curve;

        public override async Awaitable ExecuteAsync() {
            if (!RequirementsValidated()) return;
            
            float t = 0f;
            Vector3 from = _A.position;
            Vector3 to = _B.position;
            _objectToMove.position = from;
            while (t < _duration) {
                await Awaitable.EndOfFrameAsync();
                t += Time.deltaTime;
                float p = t / _duration;
                if (_useCurve) p = _curve.Evaluate(p);
                _objectToMove.position = Vector3.Lerp(from, to, p);
            }
            _objectToMove.position = to;
        }

        public override bool RequirementsValidated() {
            return _objectToMove && _A && _B;
        }

        private void OnValidate() {
            if (!RequirementsValidated()) {
                name = $"Move {_objectToMove.name} from {_A.name} to {_B.name}";
            }
        }
    }
}
