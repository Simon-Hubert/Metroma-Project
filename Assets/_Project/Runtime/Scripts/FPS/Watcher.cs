using System;
using UnityEngine;
using UnityEngine.Events;

namespace Metroma
{
    public class Watcher : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform _anchor;

        [Header("Params")] 
        [SerializeField] private LayerMask _layer;
        [SerializeField] private float _maxDist;
        [SerializeField] private bool _logs;
        [SerializeField] private UnityEvent UnityOnWatching;
        [SerializeField] private UnityEvent UnityOnStopWatching;
        
        private Watchable _cachedWatchable;

        private void OnValidate()
        {
            if (_maxDist <= 0) _maxDist = 0.5f;
        }

        private void Start()
        {
            if(_anchor == null) _anchor = transform;
        }

        private void FixedUpdate()
        {
            TryWatchAdd();
        }

        private void TryWatchAdd()
        {
            RaycastHit hit = new RaycastHit();
            if (Physics.Raycast(_anchor.position, _anchor.forward, out hit, _maxDist, _layer))
            {
                //if(_logs) Debug.Log($"[Watcher] Raycast hit: {hit.collider.gameObject.name}");
                if (hit.collider.gameObject.TryGetComponent<Watchable>(out Watchable watchable) )
                {
                    if (_cachedWatchable == watchable) {
                        return;
                    }

                    if (_cachedWatchable != null)
                    {
                        if(_logs) Debug.Log($"[Watcher] Canceling watching on: {_cachedWatchable.name}");
                        _cachedWatchable.CancelWatching();
                        UnityOnStopWatching?.Invoke();
                    }
                    _cachedWatchable = watchable;
                    if(_logs) Debug.Log($"[Watcher] Watching on: {_cachedWatchable.name}");
                    _cachedWatchable.Watch();
                    UnityOnWatching?.Invoke();
                    return;
                }
            }

            if (!_cachedWatchable) {
                return;
            }
            
            if(_logs) Debug.Log($"[Watcher] Canceling watching on: {_cachedWatchable.name}");
            _cachedWatchable.CancelWatching();
            UnityOnStopWatching?.Invoke();
            _cachedWatchable = null;

        }

        private void OnDrawGizmos()
        {
            if (_anchor == null) return;
            Gizmos.color = Color.red;
            Gizmos.DrawRay(_anchor.position, _anchor.forward * _maxDist);
        }
    }
}
