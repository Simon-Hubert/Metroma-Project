using System;
using UnityEngine;
using System.Collections.Generic;

namespace Metroma
{
    [Serializable]
    public struct ParalaxeDecor
    {
        public Transform transform;

        public float separation;
        public float paralaxe;

        public Vector3 initPos;
        public float progression;
    }
    
    public class MoveBuildings : MonoBehaviour
    {
        [SerializeReference] private AnimCurveTransition _animTransition;
        [SerializeField] private float _speed;
        
        [SerializeField] private List<ParalaxeDecor> _paralaxes = new List<ParalaxeDecor>();

        private void Start()
        {
            for (int i = 0; i < _paralaxes.Count; i++) {
                ParalaxeDecor decor = _paralaxes[i];
                
                decor.initPos =  decor.transform.position;
                decor.progression = 0;

                _paralaxes[i] = decor;
            }
        }

        private void FixedUpdate() {
            if (!_animTransition) return;
            
            float delta = Time.fixedDeltaTime;
            for (int i = 0; i < _paralaxes.Count; i++) {
                ParalaxeDecor decor = _paralaxes[i];
                
                decor.progression = Mathf.Repeat(decor.progression + (_speed * delta * decor.paralaxe * _animTransition.GetValue), decor.separation);
                decor.transform.position = new Vector3(decor.initPos.x + decor.progression, decor.transform.position.y, decor.transform.position.z);

                _paralaxes[i] = decor;
            }
        }
    }
}
