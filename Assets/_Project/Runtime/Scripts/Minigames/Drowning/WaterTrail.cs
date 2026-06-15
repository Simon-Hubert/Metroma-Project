using System;
using UnityEngine;

namespace Metroma
{
    public class WaterTrail : MonoBehaviour
    {
        [SerializeField] private Rigidbody2D _player;
        [SerializeField] private float _speedToStop;
        [Space(7)]
        [SerializeField] private TrailRenderer _trail;
        [SerializeField] private float _startAlpha;
        [SerializeField] private float _endAlpha;
        [Space(7)]
        

        private void FixedUpdate()
        {
            //float alpha = Mathf.Lerp();
            _trail.startColor = new Color(_trail.startColor.r, _trail.startColor.g, _trail.startColor.b, 0f);
        }
    }
}
