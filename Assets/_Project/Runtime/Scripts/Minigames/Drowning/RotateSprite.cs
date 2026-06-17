using System;
using NaughtyAttributes;
using SMath;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Serialization;

namespace Metroma
{
    public class RotateSprite : MonoBehaviour
    {
        private enum SpriteDirection
        {
            UP,
            UP_SIDE,
            SIDE,
            DOWN_SIDE,
            DOWN
        }
        
        [SerializeField] private FreeRoamAControllable _ctrl;
        [SerializeField] private SpriteRenderer _ctrlSpriteRenderer;

        [Header("Direction Sprites")]
        [SerializeField] private Sprite _upSprite;
        [SerializeField] private Sprite _upSideSprite;
        [SerializeField] private Sprite _sideSprite;
        [SerializeField] private Sprite _downSideSprite;
        [SerializeField] private Sprite _downSprite;
        [Space(7)]
        [SerializeField, ReadOnly] private float _currentDir;
        private SpriteDirection GetDir(float angle) {
            switch (Mathf.Abs(angle))
            {
                case <= 22.5f:
                    return SpriteDirection.UP;
                case <= 67.5f :
                    return SpriteDirection.UP_SIDE;
                case <= 112.5f :
                    return SpriteDirection.SIDE;
                case <= 157.5f :
                    return SpriteDirection.DOWN_SIDE;
                default :
                    return SpriteDirection.DOWN;
            }
        }
        
        [Header("SpinTrail")]
        [SerializeField] private Transform _trail;
        [SerializeField] private float _trailDistance = -5f;
        [SerializeField] private Vector2 _trailOffset;
        
        [Header("Swirls")]
        [SerializeField] private Transform _swirls;
        [SerializeField] private float _swirlsDistance = -7f;
        [SerializeField] private Vector2 _swirlsOffset;
        
        private SecondOrderDynamics<Vector2> _dynamics;
        [Space(10)]
        [SerializeField] private float _smoothF = 0.5f;
        [SerializeField] private float _smoothZ = 1f;
        [SerializeField] private float _smoothR = 0f;


        private void Start()
        {
            _ctrlSpriteRenderer.sprite = _downSprite;
            _ctrlSpriteRenderer.transform.localScale = Vector3.one;
            _dynamics = new SecondOrderDynamics<Vector2>(_smoothF, _smoothZ, _smoothR, Vector2.up * _trailDistance + _trailOffset, new Linear2D());
        }

        private void FixedUpdate() {
            float signedAngle = Vector2.SignedAngle(Vector2.up, _ctrl.GetDirection);
            float angle = Vector2.Angle(Vector2.up, _ctrl.GetDirection);

            SpriteDirection dir = GetDir(signedAngle);
            if (dir != GetDir(_currentDir)) {
                switch (dir) {
                    case SpriteDirection.UP:
                        _ctrlSpriteRenderer.sprite = _upSprite;
                        break;
                    case SpriteDirection.UP_SIDE:
                        _ctrlSpriteRenderer.sprite = _upSideSprite;
                        break;
                    case SpriteDirection.SIDE:
                        _ctrlSpriteRenderer.sprite = _sideSprite;
                        break;
                    case SpriteDirection.DOWN_SIDE:
                        _ctrlSpriteRenderer.sprite = _downSideSprite;
                        break;
                    case SpriteDirection.DOWN:
                    default :
                        _ctrlSpriteRenderer.sprite = _downSprite;
                        break;
                }
            }

            _ctrlSpriteRenderer.transform.localScale = new Vector3( (dir == SpriteDirection.UP || dir == SpriteDirection.DOWN) ? 1 : -Mathf.Sign(signedAngle), 1, 1);

            _trail.localPosition = _dynamics.Update(Time.fixedDeltaTime,
                new Vector2(
                    -Mathf.Sin(Mathf.Deg2Rad * signedAngle) * _trailDistance,
                    Mathf.Cos(Mathf.Deg2Rad * signedAngle) * _trailDistance) + _trailOffset);
            
            _swirls.localPosition =  new Vector2(
                Mathf.Sin(Mathf.Deg2Rad * signedAngle) * _swirlsDistance, 
                -Mathf.Cos(Mathf.Deg2Rad * signedAngle) * _swirlsDistance) + _swirlsOffset;
            _swirls.rotation = Quaternion.Euler(0, 0, signedAngle);
            
            _currentDir = signedAngle;
        }
    }
}
