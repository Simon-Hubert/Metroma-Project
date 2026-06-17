using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.VFX;
using Random = UnityEngine.Random;

namespace Metroma
{
    public class Nuag : MonoBehaviour
    {
        [SerializeField] private float _speed = 9f;
        [SerializeField] private float _duration = 1.2f;
        [SerializeField] private SpriteRenderer _sr;

        [SerializeField] private Sprite[] _sprites;
        private Collider2D _col;
        private float _t;
        
        [SerializeField] private ParticleSystem _vfx;
        [SerializeField] private UnityEvent _onHit;

        private void OnEnable() {
            _t = 0;
            _sr.sprite = _sprites[Random.Range(0, _sprites.Length)];
        }

        private void Awake() {
            _col = GetComponent<Collider2D>();
        }
        
        void Update() {
            transform.position += Vector3.left * (_speed * Time.deltaTime);
            _t += Time.deltaTime;
            if (_t > _duration) {
                gameObject.SetActive(false);
            }
        }

        public void SetColor(Color color) {
            _sr.color = color;
        }

        public void SetColliderActive(bool active) {
            if (!_col) return;
            _col.enabled = active;
        }
        

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.tag == "Player")
            {
                _vfx.transform.position = transform.position + Vector3.left;
                _vfx.transform.rotation = Quaternion.Euler(other.transform.position - transform.position);
                
                _vfx.Play();
                _onHit?.Invoke();
                gameObject.SetActive(false);
            }
        }
    }
}
