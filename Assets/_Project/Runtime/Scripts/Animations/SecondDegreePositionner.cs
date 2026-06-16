using SMath;
using static UnityEngine.Mathf;
using UnityEngine;

namespace Metroma
{
    public class SecondDegreePositionner : MonoBehaviour
    {
        [SerializeField] private Transform _target;
        [SerializeField] private float _f, _z, _r;
        [SerializeField] private float _maxRadius;
        private SecondOrderDynamicsCustom<Vector3> _dynamics;
        
        
        private void Start() {
            _dynamics = new SecondOrderDynamicsCustom<Vector3>(_f, _z, _r, _target.position, new Linear3D());
        }

        private void Update() {
            Vector3 position = _target.position;
            transform.position = _dynamics.Update(Time.deltaTime, position);

            if (!(_maxRadius > 0) || !((transform.position - position).magnitude > _maxRadius)) {
                return;
            }
            
            Vector3 dir = transform.position - position;
            dir.Normalize();
            transform.position = position + dir * _maxRadius;
            _dynamics.UpdateCurrent(transform.position);
        }
    }
    
    
    public class SecondOrderDynamicsCustom<T>
    {
        private readonly ILinearMath<T> _math;
        
        private T _xp;
        private T _y, _yd;
        private float _k1, _k2, _k3;

        public SecondOrderDynamicsCustom(float f, float z, float r, T x0, ILinearMath<T> math){
            _math = math;
            
            _k1 = z / (PI * f);
            _k2 = 1 / ((2 * PI * f) * (2 * PI * f));
            _k3 = r * z / (2 * PI * f);

            _xp = x0;
            _y = x0;
            _yd = _math.Zero();
        }
        
        public T Update(float deltaTime, T x){
            float k2Stable = Max(_k2, 1.1f * (deltaTime * deltaTime / 4 + deltaTime * _k1 / 2));
            T xd = _math.Mul(_math.Sub(x, _xp), 1f/ deltaTime);
            _xp = x;

            
            _y = _math.Add(_y, _math.Mul(deltaTime, _yd));
            _yd = _math.Add(_yd, _math.Mul(_math.Mul(deltaTime, _math.Sub(_math.Sub(_math.Add(x, _math.Mul(_k3, xd)), _y), _math.Mul(_k1, _yd))), 1f/k2Stable));
            return _y;
        }
        
        public T Update(float deltaTime, T x, T xd){
            float k2Stable = Max(_k2, 1.1f * (deltaTime * deltaTime / 4 + deltaTime * _k1 / 2));
            _xp = x;

            _y = _math.Add(_y, _math.Mul(deltaTime, _yd));
            _yd = _math.Add(_yd, _math.Mul(_math.Mul(deltaTime, _math.Sub(_math.Sub(_math.Add(x, _math.Mul(_k3, xd)), _y), _math.Mul(_k1, _yd))), 1f/k2Stable));
            return _y;
        }

        public void UpdateCurrent(T y) {
            _y = y;
        }
    }
    
    
}
