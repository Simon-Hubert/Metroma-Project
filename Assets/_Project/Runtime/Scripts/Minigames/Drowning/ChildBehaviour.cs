using System;
using System.Collections;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Serialization;
using Vector2 = UnityEngine.Vector2;

namespace Metroma
{
    public class ChildBehaviour : MonoBehaviour
    {
        [Serializable]
        private enum ChildState {
            Idle,
            Fear,
            Out
        }

        [SerializeField] private ChildManager _manager;
        [Space(7)]
        [SerializeField] private Rigidbody2D _rigidbody2D;
        [SerializeField] private Transform _childVisual;
        
        [Header("Parameters")]
        [SerializeField] private float _distanceWithShark;
        [SerializeField] private float _fleeForce;
        [SerializeField, ReadOnly] private ChildState _state;

        [FormerlySerializedAs("outAngle")]
        [Header("OutAnim")]
        [SerializeField] private float _outAngle = 45f;
        [SerializeField] private float _minDistance = 5f;
        [SerializeField] private float _timeToEscape = 1f;
        [SerializeField] private float _speedToEScape = 100f;
        
        // [SerializeField] private float _ejectionDistance;
        // [SerializeField] private float _ejectionSpeed;
        // [Space(5)]
        // [SerializeField] private float _jumpHeight;
        // [SerializeField] private AnimationCurve _jumpCurve;

        private void OnValidate() {
            if (!_manager && transform.parent) {
                transform.parent.TryGetComponent(out _manager);
            }
        }

        private void Update() {
            if (!_manager || _state == ChildState.Out) return;
            
            if (Vector2.Distance(_manager.GetPlayerPos, transform.position) <= _distanceWithShark) {
                
            }
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.transform.GetInstanceID() != _manager.GetPlayer.GetInstanceID()) return;

            _state = ChildState.Fear;
            Vector2 a = (Vector2)transform.position - _manager.GetPlayerPos;
            
            float angle = -_outAngle;
            Vector2 outDir = Vector2.zero;
            
            for (int i = 0; i < 3; i++)
            {
                angle += _outAngle * i;

                outDir = new Vector2(
                    a.x * Mathf.Cos(angle) - a.y * Mathf.Sin(angle),
                    a.x * Mathf.Sin(angle) + a.y * Mathf.Cos(angle));

                RaycastHit2D hit = Physics2D.Raycast(transform.position, outDir);
                if (!hit || (hit && hit.distance > _minDistance)) {
                    break;
                }
            }

            StartCoroutine(OnAnimCoroutine(outDir));
        }

        private IEnumerator OnAnimCoroutine(Vector2 dir)
        {
            float time = _timeToEscape;

            Vector2 start = transform.position;
            Vector2 end = (Vector2)transform.position + dir * (_timeToEscape * _speedToEScape);

            while (time > 0f)
            {
                time -= Time.fixedDeltaTime;
                transform.position = Vector2.Lerp(start, end, 1 - (time / _timeToEscape));

                yield return new WaitForFixedUpdate();
            }

            _manager.OutChild();
            gameObject.SetActive(false);
            yield break;
        }
        
        // private IEnumerator OutAnimCoroutine(Vector2 dir) {
        //     float totalTime = _ejectionDistance / _ejectionSpeed;
        //     float time = totalTime;
        //
        //     Vector2 start = transform.position;
        //     Vector2 end = start + (dir * _ejectionDistance);
        //
        //     while (time > 0f) {
        //         time -= Time.fixedDeltaTime;
        //         float prog = 1 - (time / totalTime);
        //
        //         float height = _jumpCurve.Evaluate(prog) * _jumpHeight;
        //
        //         transform.position = Vector2.Lerp(start, end, prog) + (Vector2.up * height);
        //         yield return new WaitForFixedUpdate();
        //     }
        //
        //     gameObject.SetActive(false);
        //     yield break;
        // }
    }
}
