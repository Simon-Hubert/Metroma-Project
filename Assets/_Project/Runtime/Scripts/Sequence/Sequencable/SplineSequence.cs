using System;
using Dreamteck.Splines;
using NaughtyAttributes;
using UnityEngine;

namespace Metroma
{
    public class SplineSequence : ASequencable
    {
        [SerializeField] private GameObject _object;
        [SerializeField] private float _duration;
        [SerializeField] SplineComputer _spline;
        [SerializeField] private bool _useCurve;
        [SerializeField, ShowIf("_useCurve")] private AnimationCurve _curve;
        
        public override async Awaitable ExecuteAsync() {
            if (!RequirementsValidated()) return;
            float t = 0f;
            while (t < _duration) {
                await Awaitable.NextFrameAsync();
                t += Time.deltaTime;
                float p = t / _duration;
                if (_useCurve) p = _curve.Evaluate(p);
                _object.transform.position = _spline.EvaluatePosition(p);
            }
        }

        public override bool RequirementsValidated() {
            return _object && _spline;
        }

        public void OnValidate() {
            if (!RequirementsValidated()) return;
            name = $"Move {_object} along {_spline}";
        }
    }
}
