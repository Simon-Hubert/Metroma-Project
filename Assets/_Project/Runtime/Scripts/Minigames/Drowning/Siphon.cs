using System;
using UnityEditor;
using UnityEngine;

namespace Metroma
{
    public class Siphon : MonoBehaviour
    {
        [SerializeField] private float _force;
        [SerializeField] private float _radius;
        [SerializeField] private AnimationCurve _forceOverDistance;

        [SerializeField] private DrowningControllable _controller;

        private void Update() {
            Vector2 dir = ((Vector2)(_controller.transform.position - transform.position)).normalized;
            float distance = (_controller.transform.position - transform.position).magnitude;
            if ( distance < _radius  && distance > 0.5f) {
                _controller.SetAdditionalForce(_force * _forceOverDistance.Evaluate(1-(distance/_radius)) * -dir);
            }
        }

        private void OnDrawGizmos() {
            #if UNITY_EDITOR
            Handles.color = Color.lawnGreen;
            Handles.DrawWireDisc(transform.position, Vector3.back, _radius);
            Handles.DrawWireDisc(transform.position, Vector3.back, 0.5f);
            #endif
        }
    }
}
