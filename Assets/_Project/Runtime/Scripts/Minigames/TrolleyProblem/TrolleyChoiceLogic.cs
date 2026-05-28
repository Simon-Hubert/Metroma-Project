using System;
using NaughtyAttributes;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Metroma
{
    public class TrolleyChoiceLogic : MonoBehaviour
    {
        [SerializeField] private TrolleyChoiceSO _trolleyChoiceSO;

        public TrolleyChoiceSO SetTrolleySO {
            set {
                if (_trolleyChoiceSO == null || value != _trolleyChoiceSO && value != null) {
                    _trolleyChoiceSO = value;
                    ResetChoiceValues();
                }
            }
        }
        public string GetChoiceId { get => _trolleyChoiceSO != null ? _trolleyChoiceSO.id : null; }

        [Header("Physics")]
        [SerializeField] private Rigidbody2D _rb2D;
        [SerializeField] private HingeJoint2D _hj2D;
        
        public bool SetHingeEnabled { set => _hj2D.enabled = value; }
        public Rigidbody2D SetHingeConnectedRB2D {set => _hj2D.connectedBody = value; } 
        public Vector2 SetHingeAnchorOffset { set => _hj2D.anchor = value; } 
        
        [Header("Visual")]
        [SerializeField] private SpriteRenderer _illustration;
        [Space(10)]
        [SerializeField] private TextMeshProUGUI _name;
        [SerializeField] private bool _displayNameRaw;
        [SerializeField] private TextMeshProUGUI _description;
        [SerializeField] private string _listMark = " -";
        [SerializeField] private bool _displayDescriptionRaw;

        private void Start()
        {
            ResetChoiceValues();
        }

        [Button]
        private void ResetChoiceValues() {
            if (!_trolleyChoiceSO) Debug.LogWarning($"{name}'s TrolleyChoiceLogic : no TrolleyChoiceSO found");
            
            _illustration.sprite = _trolleyChoiceSO ? _trolleyChoiceSO.illustration : null;

            if (_trolleyChoiceSO) {
                _name.text = _displayNameRaw ? _trolleyChoiceSO.name : _trolleyChoiceSO.name.ToUpper();

                _description.text = "";
                foreach (string text in _trolleyChoiceSO.elements) {
                    _description.text += _displayDescriptionRaw ? $"{text}\n" : $"{_listMark} {text}\n";
                }
            }
            else {
                _name.text = "Lorem Ipsum";
                _description.text = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed non risus. Suspendisse lectus tortor, dignissim sit amet, adipiscing nec, ultricies sed, dolor.";
            }
        }

        public void ResetPhysicsValues(Vector2 minSpeed, Vector2 maxSpeed) {
            Vector2 force = new Vector2(Random.Range(minSpeed.x, maxSpeed.x), Random.Range(minSpeed.y, maxSpeed.y));
            ResetPhysicsValues(force);
        }
        public void ResetPhysicsValues(Vector2 initSpeed) {
            transform.rotation = Quaternion.Euler(0, 0, 0);
            
            _rb2D.linearVelocity = initSpeed;
            _rb2D.angularVelocity = 0;

            _hj2D.enabled = true;
        }
    }
}
