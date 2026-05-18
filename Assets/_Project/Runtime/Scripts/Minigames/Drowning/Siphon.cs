using System;
using UnityEditor;
using UnityEngine;

namespace Metroma
{
    public class Siphon : MonoBehaviour
    {
        [SerializeField] private float _force;
        [SerializeField] private float _radius;
        [SerializeField] private float _deathTimer = 2f;
        [SerializeField] private float _innerRadius = 0.5f;
        [SerializeField] private AnimationCurve _forceOverDistance;

        [SerializeField] private DrowningControllable _controller;
        private float _centerCounter;

        private void OnEnable() { //TODO C'est absolument degeulasse
            _controller.Init();
        }

        private void Update() {
            Vector2 dir = ((Vector2)(_controller.transform.position - transform.position)).normalized;
            float distance = (_controller.transform.position - transform.position).magnitude;
            if ( distance < _radius  && distance > _innerRadius) {
                _controller.SetAdditionalForce(_force * _forceOverDistance.Evaluate(1-(distance/_radius)) * -dir);
            }
            if (distance < _innerRadius) {
                _centerCounter += Time.deltaTime;
            }
            else {
                _centerCounter = 0;
            }

            if (_centerCounter >= _deathTimer) {
                _centerCounter = 0;
                _controller.Respawn();
            }
        }

        private void OnDrawGizmos() {
            #if UNITY_EDITOR
            Handles.color = Color.lawnGreen;
            Handles.DrawWireDisc(transform.position, Vector3.back, _radius);
            Handles.DrawWireDisc(transform.position, Vector3.back, _innerRadius);
            #endif
        }
    }
}
