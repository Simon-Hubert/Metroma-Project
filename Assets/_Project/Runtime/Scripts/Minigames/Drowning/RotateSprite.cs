using System;
using NaughtyAttributes;
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

        private void Start()
        {
            _ctrlSpriteRenderer.sprite = _downSprite;
            _ctrlSpriteRenderer.transform.localScale = Vector3.one;
        }

        private void FixedUpdate() {
            float angle = Vector2.SignedAngle(Vector2.up, _ctrl.GetDirection);

            SpriteDirection dir = GetDir(angle);
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

            _ctrlSpriteRenderer.transform.localScale = new Vector3( (dir == SpriteDirection.UP || dir == SpriteDirection.DOWN) ? 1 : -Mathf.Sign(angle), 1, 1);
            
            _currentDir = angle;
        }
    }
}
