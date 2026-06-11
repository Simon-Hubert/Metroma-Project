using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

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

        [Header("OutAnim")]
        [SerializeField] private float _ejectionDistance;
        [SerializeField] private float _ejectionSpeed;
        [Space(5)]
        [SerializeField] private float _jumpHeight;
        [SerializeField] private AnimationCurve _jumpCurve;

        private void OnValidate() {
            if (!_manager) {
                transform.parent.TryGetComponent(out _manager);
            }
        }

        private void Update() {
            if (!_manager || _state == ChildState.Out) return;
            
            if (Vector2.Distance(_manager.GetPlayerPos, transform.position) <= _distanceWithShark) {
                
            }

            if (_state == ChildState.Fear) {
                _rigidbody2D.AddForce((_manager.GetPlayerPos - (Vector2)transform.position) * _fleeForce);
            }
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (other.transform.GetInstanceID() != _manager.GetPlayer.GetInstanceID()) return;
            
            Vector2 outDir = other.GetContact(0).point - (Vector2)transform.position;
            
            
        }

        private IEnumerator OutAnimCoroutine(Vector2 dir) {
            float totalTime = _ejectionDistance / _ejectionSpeed;
            float time = totalTime;

            Vector2 start = transform.position;
            Vector2 end = start + (dir * _ejectionDistance);

            while (time > 0f) {
                time -= Time.fixedDeltaTime;
                float prog = 1 - (time / totalTime);

                float height = _jumpCurve.Evaluate(prog) * _jumpHeight;

                transform.position = Vector2.Lerp(start, end, prog) + (Vector2.up * height);
                yield return new WaitForFixedUpdate();
            }

            gameObject.SetActive(false);
            yield break;
        }
    }
}
