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
        [SerializeField] private SpriteRenderer _childVisual;
        
        [Header("Parameters")]
        [SerializeField] private float _distanceWithShark;
        [SerializeField] private float _fleeForce;
        [SerializeField, ReadOnly] private ChildState _state;
        private ChildState SetChildState {
            set {
                if (_state != value) {
                    _state = value;
                    StateFeedback(_state);
                }
            }
        }
        
        [Header("OutAnim")]
        [SerializeField] private float _outAngle = 45f;
        [SerializeField] private float _minDistance = 5f;
        [SerializeField] private float _timeToEscape = 1f;
        [SerializeField] private float _speedToEScape = 100f;

        [Header("Sprites")] 
        [SerializeField] private Sprite _spriteIdle;
        [Space(5)]
        [SerializeField] private Sprite _spriteFear1;
        [SerializeField] private Sprite _spriteFear2;
        [SerializeField] private float _spritesLoop = 0.5f;
        private float _spritesLoopCurrent = 0f;
        [Space(5)]
        [SerializeField] private Sprite _spriteRun;
        
        [Header("VFX")]
        [SerializeField] private ParticleSystem _fearVFX;


        private void OnValidate() {
            if (!_manager && transform.parent) {
                transform.parent.TryGetComponent(out _manager);
            }
        }

        private void Update() {
            if (!_manager || _state == ChildState.Out) return;
            
            if (_state == ChildState.Fear) {
                _spritesLoopCurrent = Mathf.Repeat(_spritesLoopCurrent += Time.deltaTime, _spritesLoop);

                if (_spritesLoopCurrent < _spritesLoop / 2 && _childVisual.sprite != _spriteFear1) {
                    _childVisual.sprite = _spriteFear1;
                } 
                else if (_spritesLoopCurrent >= _spritesLoop / 2 && _childVisual.sprite != _spriteFear2) {
                    _childVisual.sprite = _spriteFear2;
                }
            }
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.transform.GetInstanceID() != _manager.GetPlayer.GetInstanceID()) return;

            SetChildState = ChildState.Fear;
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

            Vector3 start = transform.position;
            Vector3 end = transform.position + (Vector3)(dir * (_timeToEscape * _speedToEScape));

            while (time > 0f)
            {
                time -= Time.fixedDeltaTime;
                transform.position = Vector3.Lerp(start, end, 1 - (time / _timeToEscape));

                yield return new WaitForFixedUpdate();
            }

            _manager.OutChild();
            gameObject.SetActive(false);
            yield break;
        }

        private void StateFeedback(ChildState state) {
            switch (state) {
                case ChildState.Fear :
                    _fearVFX.Play();
                    break;
                case ChildState.Out :
                    if (!_fearVFX.isStopped) _fearVFX.Stop();
                    _childVisual.sprite = _spriteRun;
                    break;
                case ChildState.Idle :
                default:
                    if (!_fearVFX.isStopped) _fearVFX.Stop();
                    _childVisual.sprite = _spriteIdle;
                    break;
            }
        }
    }
}
