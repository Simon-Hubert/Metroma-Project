using System;
using NaughtyAttributes;
using TMPro;
using UnityEngine;

namespace Metroma
{
    public class TrolleyChoiceLogic : MonoBehaviour
    {
        [SerializeField] private TrolleyChoiceSO _trolleyChoiceSO;

        public TrolleyChoiceSO SetTrolleySO {
            set {
                if (_trolleyChoiceSO == null || value != _trolleyChoiceSO) {
                    _trolleyChoiceSO = value;
                    ResetChoiceValues();
                }
            }
        }
        public string GetChoiceId { get => _trolleyChoiceSO != null ? _trolleyChoiceSO.id : null; }

        [Header("Visual")]
        [SerializeField] private SpriteRenderer _background;
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
            _background.sprite = _trolleyChoiceSO.background;
            _illustration.sprite = _trolleyChoiceSO.illustration;

            _name.text = _displayNameRaw ? _trolleyChoiceSO.name : _trolleyChoiceSO.name.ToUpper();

            _description.text = "";
            foreach (string text in _trolleyChoiceSO.elements) {
                _description.text += _displayDescriptionRaw ? text : $"{_listMark} {text}\n";
            }
        }
    }
}
