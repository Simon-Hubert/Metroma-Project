using System.Collections;
using MoreMountains.Feedbacks;
using UnityEngine;

namespace Metroma
{
    public class ViewWiggler : MonoBehaviour
    {
        [SerializeField] private CamWiggleAnchor _anchor;
        [SerializeField] private float _maxDuration;
        [SerializeField] private AnimationCurve _curve;

        private bool _isWatching;
        private Vector3 _original;
        private Coroutine _watchingRoutine;

        public void StartedWatching() {
            if (_watchingRoutine != null) {
                CancelledWatching();
            }
            _watchingRoutine = StartCoroutine(OnWatchRoutine());
        }

        public void CancelledWatching() {
            if (_watchingRoutine != null) {
                StopCoroutine(_watchingRoutine);
                _watchingRoutine = null;
            }
            MMWiggle wiggle = _anchor.Target;
            wiggle.PositionWiggleProperties.AmplitudeMax = _original;
            wiggle.enabled = false;
        }

        IEnumerator OnWatchRoutine() {
            float t = 0;
            MMWiggle wiggle = _anchor.Target;
            wiggle.enabled = true;
            _original = wiggle.PositionWiggleProperties.AmplitudeMax;
            while (t < _maxDuration) {
                t += Time.deltaTime;
                float p = t / _maxDuration;
                p = _curve.Evaluate(p);
                wiggle.PositionWiggleProperties.AmplitudeMax = _original * p;
                wiggle.Initialization();
                yield return null;
            }
        }
    }
}
