using System;
using UnityEngine;
using UnityEngine.VFX;

namespace Metroma
{
    public class Nuag : MonoBehaviour
    {
        [SerializeField] private float _speed = 9f;
        [SerializeField] private float _duration = 1.2f;
        private float _t;
        
        [SerializeField] private VisualEffect _vfx;

        private void OnEnable() {
            _t = 0;
        }

        void Update() {
            transform.position += Vector3.left * (_speed * Time.deltaTime);
            _t += Time.deltaTime;
            if (_t > _duration) {
                gameObject.SetActive(false);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.tag == "Player")
            {
                _vfx.transform.position = transform.position;
                _vfx.Play();
                gameObject.SetActive(false);
            }
        }
    }
}
