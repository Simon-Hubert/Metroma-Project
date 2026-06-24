using System;
using System.Collections;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;
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
        [SerializeField] private SpriteRenderer _frontBoeyVisual;
        
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
        [SerializeField] private UnityEvent OuStateIdle;
        [SerializeField] private UnityEvent OuStateFear;
        [SerializeField] private UnityEvent OuStateOut;
        
        [Header("OutAnim")]
        [SerializeField] private float _escapeSpeed = 50f;
        [SerializeField] private float _escapeAnimSpeed = 100f;
        [SerializeField] private float _escapeAnimDuration = 1f;
        [SerializeField] private Animator _animator;
        [SerializeField] private UnityEvent OnEnd;

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
                
                Vector2 fleeVector = ((Vector2)transform.position - _manager.GetPlayerPos).normalized;
                transform.position += (Vector3)fleeVector * (_escapeSpeed * Time.deltaTime);
            }
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_state == ChildState.Out) return;
            if (other.transform.GetInstanceID() != _manager.GetPlayer.GetInstanceID()) return;
            
            SetChildState = ChildState.Fear;
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (_state == ChildState.Out) return;
            if (other.transform.GetInstanceID() != _manager.GetPoolBorder.GetInstanceID()) return;
            
            SetChildState = ChildState.Out;
            StartCoroutine(endAnimationRoutine());
        }

        private IEnumerator endAnimationRoutine() {
            _animator.SetBool("Splash", true);

            float duration = _escapeAnimDuration;
            while (duration > 0) {
                duration -= Time.deltaTime;
                
                Vector2 fleeVector = ((Vector2)_childVisual.transform.position - _manager.GetPlayerPos).normalized;
                _childVisual.transform.position += new Vector3(fleeVector.x, fleeVector.y) * (_escapeAnimSpeed * Time.deltaTime);
                yield return new WaitForEndOfFrame();
            }

            _childVisual.gameObject.SetActive(false);
            _manager.OutChild();
            OnEnd.Invoke();
            yield break;
        }
        
        private void StateFeedback(ChildState state) {
            switch (state) {
                case ChildState.Fear :
                    _fearVFX.Play();
                    _frontBoeyVisual.gameObject.SetActive(true);
                    break;
                case ChildState.Out :
                    if (!_fearVFX.isStopped) _fearVFX.Stop();
                    OuStateOut?.Invoke();
                    _childVisual.sprite = _spriteRun;
                    _frontBoeyVisual.gameObject.SetActive(true);
                    break;
                case ChildState.Idle :
                default:
                    if (!_fearVFX.isStopped) _fearVFX.Stop();
                    _childVisual.sprite = _spriteIdle;
                    _frontBoeyVisual.gameObject.SetActive(false);
                    break;
            }
        }
    }
}
