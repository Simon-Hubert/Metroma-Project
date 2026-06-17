using System;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Metroma
{
    public class ObstacleManager : MonoBehaviour
    {
        [SerializeField, MinMaxSlider(-10, 10)] private Vector2 _maxRange;
        [SerializeField, MinMaxSlider(-1, 20)] private Vector2 _maxZRange;
        [SerializeField, MinMaxSlider(0.01f,10)] private Vector2 _nuagParSec;
        [SerializeField] private Gradient _colorOverDepth;
        [SerializeField, Range(0,1)] private float _collisionThreshhold;
        private Nuag[] _children;
        private float _accumulator;
        private float _nextTime;
        private int _currentIndex;
        
        private void Start() {
            _children = (from Transform obj in transform select obj.GetComponent<Nuag>()).ToArray();
            foreach (Nuag child in _children) {
                if (child == null) continue;
                child.gameObject.SetActive(false);
            }
        }

        private void Update() {
            _accumulator += Time.deltaTime;
            if (!(_accumulator > _nextTime)) return;
            _accumulator = 0;
            _nextTime = Random.Range(1f/_nuagParSec.x, 1f/_nuagParSec.y);
            Nuag current =_children[_currentIndex];
            _currentIndex++;
            _currentIndex = _currentIndex % (_children.Length-1);
            current.gameObject.SetActive(true);
            float height = Random.Range(_maxRange.x, _maxRange.y);
            float z = Random.Range(_maxZRange.x, _maxZRange.y);
            float d = Mathf.InverseLerp(_maxZRange.x, _maxZRange.y, z);
            current.SetColliderActive(d < _collisionThreshhold);
            current.SetColor(_colorOverDepth.Evaluate(d));
            current.transform.position = transform.position + new Vector3(0, height, z);
        }
        
    }
}
