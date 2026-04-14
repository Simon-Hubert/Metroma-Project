using System;
using UnityEngine;

namespace Metroma
{
    public class SmileFeedback : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _sr;
        [SerializeField] private Gradient _gradient;
        [SerializeField] private ScratchControllable _scratchControllable;
        [SerializeField] private ScratchCondition _scratchCondition;

        private void OnEnable() {
            _scratchControllable.OnScratch += UpdateVisual;
        }

        private void OnDisable() {
            _scratchControllable.OnScratch -= UpdateVisual;
        }

        private void UpdateVisual() {
            _sr.color = _gradient.Evaluate(_scratchControllable.TotalDistance / _scratchCondition.Target);
        }
    }
}
