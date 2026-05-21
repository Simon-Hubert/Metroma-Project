using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Metroma
{
    public class CitrusManager : MonoBehaviour
    {
        [SerializeField] private GameObject[] _citruses;
        [SerializeField] private float _startZ;
        [SerializeField] private float _endZ;
        [SerializeField] private AnimationCurve _positionCurve;
        [SerializeField] private AnimationCurve _scaleCurve;
        [SerializeField, MinMaxSlider(1, 5)] private Vector2 _durationRange;
        [SerializeField, MinMaxSlider(0, 1)] private Vector2 _startRange;

        private void Start() {
            _citruses = _citruses.OrderByDescending(go => go.GetComponent<SpriteRenderer>().sortingOrder).ToArray();
        }
        
        public void Play() {
            Debug.Log($"{_citruses.Length}");
            for (int i = 0; i < _citruses.Length; i++) {
                Debug.Log($"{i}, {_citruses[i]}");
                _citruses[i].SetActive(true);
                Vector3 pos = _citruses[i].transform.position;
                pos.z = _startZ;
                _citruses[i].transform.position = pos;
                float waitTime = i * (1f/_citruses.Length) + Random.Range(_startRange.x, _startRange.y);
                float duration = Random.Range(_durationRange.x, _durationRange.y);
                StartCoroutine(PlayRoutine(_citruses[i], waitTime, duration));
            }
        }
        
        private IEnumerator PlayRoutine(GameObject go, float waitTime, float duration) {
            Debug.Log($"{go.name}");
            float t = 0;
            while (t < waitTime) {
                yield return null;
                t += Time.deltaTime;
            }
            t = 0;
            Vector3 baseScale = go.transform.localScale;
            while (t < duration) {
                yield return null;
                t += Time.deltaTime;
                float p = t / duration;
                Vector3 position = go.transform.position;
                position.z = Mathf.Lerp(_startZ, _endZ, _positionCurve.Evaluate(p));
                go.transform.position = position;
                go.transform.localScale = baseScale * _scaleCurve.Evaluate(p);
            }
            Vector3 pos = go.transform.position;
            pos.z = _endZ;
            go.transform.position = pos;
            go.transform.localScale = baseScale;
        }
    }
}
