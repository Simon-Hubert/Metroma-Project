using System;
using System.Collections.Generic;
using NaughtyAttributes;
using Shapes;
using UnityEngine;

namespace Metroma
{
    public class NanterreLaFolie : ImmediateModeShapeDrawer
    {
        [SerializeField] private bool _useSpriteMask = false;
        [SerializeField, ShowIf("_useSpriteMask")] private GameObject _spritePrefab;
        
        private List<Vector3> positions = new List<Vector3>();
        private bool _enabled = false;
        private Vector3 _lastPosition;

        public void Enable(bool enable) => _enabled = enable;
        public override void DrawShapes(Camera cam)
        {
            if (!_useSpriteMask)
            {
                using (Draw.Command(cam))
                {
                    for (int i = 0; i < positions.Count; i++)
                    {
                        Draw.Disc(positions[i], 1f);    
                    }
                }
            }
        }

        private void Update()
        {
            if (_useSpriteMask && _enabled && transform.position != _lastPosition)
            {
                Instantiate(_spritePrefab, transform.position, Quaternion.identity);
                _lastPosition = transform.position;
            }
            else
            {
                positions.Add(transform.position);
            }
        }
    }
}
