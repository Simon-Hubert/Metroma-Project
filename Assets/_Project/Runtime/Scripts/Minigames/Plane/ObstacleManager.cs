using System;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Metroma
{
    public class ObstacleManager : MonoBehaviour
    {
        [SerializeField, MinMaxSlider(-10, 10)] private Vector2 _maxRange;
        [SerializeField, MinMaxSlider(0,3)] private Vector2 _intervalRange;
        private GameObject[] _children;
        private float _accumulator;
        private float _nextTime;
        private int _currentIndex;
        
        private void Start() {
            _children = (from Transform obj in transform select obj.gameObject).ToArray();
            foreach (GameObject child in _children) {
                child.SetActive(false);
            }
        }

        private void Update() {
            _accumulator += Time.deltaTime;
            if (!(_accumulator > _nextTime)) return;
            _accumulator = 0;
            _nextTime = Random.Range(_intervalRange.x, _intervalRange.y);
            GameObject current =_children[_currentIndex];
            _currentIndex++;
            _currentIndex = _currentIndex % _children.Length;
            current.SetActive(true);
            float height = Random.Range(_maxRange.x, _maxRange.y);
            current.transform.position = transform.position + new Vector3(0, height, 0);
        }
        
    }
}
