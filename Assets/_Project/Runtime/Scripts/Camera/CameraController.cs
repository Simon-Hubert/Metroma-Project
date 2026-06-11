using System;
using NaughtyAttributes;
using SMath;
using UnityEngine;
using static UnityEngine.Mathf;

namespace Metroma
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private Camera _cam;
        [SerializeField] private AView _view;
        [SerializeField] private bool _dampen = true;
        [SerializeField, ShowIf("_dampen")] private float _f = 1;
        [SerializeField, ShowIf("_dampen")] private float _z = 2;
        [SerializeField, ShowIf("_dampen")] private float _r = 0;

        private bool _cut = false;
        
        private CameraConfiguration _current;
        private SecondDegreeSmoother _smoother;
        private SecondOrderDynamics<Vector2> _dynamics2D;
        

        private void Start() {
            _smoother = new SecondDegreeSmoother(_f, _z, _r);
            _current = _view.GetConfiguration();
        }

        private void LateUpdate() {
            if (_dampen && !_cut) {
                _current = _smoother.Smooth(_current, _view.GetConfiguration());
            }
            else {
                _cut = false;
                _current = _view.GetConfiguration();
            }
            CameraHelpers.ApplyConfiguration(_cam, _current);
            
        }
        
        public void ChangeView(AView view) => _view = view;

        public void CutToView(AView view) {
            _view = view;
            _cut = true;
        }
    }
    
    public class SecondDegreeSmoother 
    {
        private CameraConfiguration _lastPos;
        private CameraConfiguration _speed;

        private Vector2 _lastYawVector;
        private Vector2 _yawSpeed;

        private readonly float _f;
        private readonly float _z;
        private readonly float _r; 
        private float _k1, _k2, _k3;
        
        public SecondDegreeSmoother(float f, float z, float r) {
            _f = f;
            _z = z;
            _r = r;
        }

        public CameraConfiguration Smooth(CameraConfiguration current, CameraConfiguration target) {
            if (Time.deltaTime <= 0f)
                return current;

            _k1 = _z / (PI * _f);
            _k2 = 1 / ((2 * PI * _f) * (2 * PI * _f));
            _k3 = _r * _z / (2 * PI * _f);
        
        
            CameraConfiguration lastSpeed = (target - _lastPos) / Time.deltaTime;
            _lastPos = target;
        
            Vector2 yawVector =  new Vector2(
                Mathf.Cos(target.Yaw * Deg2Rad),
                Mathf.Sin(target.Yaw * Deg2Rad));
        
            Vector2 currentYawVector =  new Vector2(
                Mathf.Cos(target.Yaw * Deg2Rad),
                Mathf.Sin(target.Yaw * Deg2Rad));


            Vector2 lastYawSpeed = (yawVector - _lastYawVector) / Time.deltaTime;
            _lastYawVector = yawVector;
        
            _yawSpeed += Time.deltaTime * (yawVector + _k3 * lastYawSpeed - currentYawVector - _k1 * _yawSpeed) / _k2;
        
            current += Time.deltaTime * _speed;
            _speed += Time.deltaTime * (target + _k3 * lastSpeed - current - _k1 * _speed) / _k2;

            currentYawVector += _yawSpeed * Time.deltaTime;
            current.Yaw = Atan2(currentYawVector.y, currentYawVector.x) * Rad2Deg;
         
            return current;
        }
    }


}
