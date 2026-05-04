using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Metroma
{
    #region Floats

    [Serializable]
    public class Float : IFloat
    {
        [SerializeField] private float value;
        public float Evaluate() {
            return value;
        }
    }
    
    [Serializable]
    public class FloatAdd : IFloat
    {
        
        [SerializeReference, ConditionAttribute(ConditionAttribute.ConditionType.FloatCalculation)] IFloat a;
        [SerializeReference, ConditionAttribute(ConditionAttribute.ConditionType.FloatCalculation)] IFloat b;
        
        public float Evaluate() {
            return a.Evaluate() + b.Evaluate();
        }
    }
    
    [Serializable]
    public class FloatMul : IFloat
    {
        [SerializeReference, ConditionAttribute(ConditionAttribute.ConditionType.FloatCalculation)] private IFloat a;
        [SerializeReference, ConditionAttribute(ConditionAttribute.ConditionType.FloatCalculation)] private IFloat b;
        
        public float Evaluate() {
            return a.Evaluate() * b.Evaluate();
        }
    }

    #endregion

    #region Ints

    [Serializable]
    public class Int : IInt
    {
        [SerializeField] private int value;
        
        public int Evaluate() {
            return value;
        }
    }
    
    [Serializable]
    public class IntAdd : IInt
    {
        
        [SerializeReference, ConditionAttribute(ConditionAttribute.ConditionType.IntCalculation)] private IInt a;
        [SerializeReference, ConditionAttribute(ConditionAttribute.ConditionType.IntCalculation)] private IInt b;
        
        public int Evaluate() {
            return a.Evaluate() + b.Evaluate();
        }
    }
    
    [Serializable]
    public class IntMul : IInt
    {
        [SerializeReference, ConditionAttribute(ConditionAttribute.ConditionType.IntCalculation)] private IInt a;
        [SerializeReference, ConditionAttribute(ConditionAttribute.ConditionType.IntCalculation)] private IInt b;
        
        public int Evaluate() {
            return a.Evaluate() * b.Evaluate();
        }
    }

    #endregion

    #region References
    
    public class FloatReference : IFloat
    {
        [SerializeField] private Object _value;
        [SerializeField] private string _variableName;

        private Func<float> _getter;
        private bool _isInitialized = false;
        
        public float Evaluate()
        {
            if (!_isInitialized)
            {
                Init();
            }
            
            if (_getter != null) return _getter.Invoke();
            
            return 0f;
        }

        private void Init()
        {
            _isInitialized = true;
            if (_value != null && !string.IsNullOrEmpty(_variableName))
            {
                Type type = _value.GetType();
                    
                var propInfo = type.GetProperty(_variableName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (propInfo != null && propInfo.CanRead)
                {
                    var getMethod = propInfo.GetGetMethod(true);
                    if (getMethod != null)
                        _getter = (Func<float>)Delegate.CreateDelegate(typeof(Func<float>), _value, getMethod);
                }
                else
                {
                    var fieldInfo = type.GetField(_variableName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (fieldInfo != null)
                    {
                        _getter = () => (float)fieldInfo.GetValue(_value);
                    }
                }
            }
        }
    }
    
    public class IntReference : IInt
    {
        [SerializeField] private Object _value;
        [SerializeField] private string _variableName;

        private Func<int> _getter;
        private bool _isInitialized = false;
        
        public int Evaluate()
        {
            if (!_isInitialized)
            {
                Init();
            }
            
            if (_getter != null) return _getter.Invoke();
            
            return 0;
        }
        
        private void Init()
        {
            _isInitialized = true;
            if (_value != null && !string.IsNullOrEmpty(_variableName))
            {
                Type type = _value.GetType();
                    
                var propInfo = type.GetProperty(_variableName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (propInfo != null && propInfo.CanRead)
                {
                    var getMethod = propInfo.GetGetMethod(true);
                    if (getMethod != null)
                        _getter = (Func<int>)Delegate.CreateDelegate(typeof(Func<float>), _value, getMethod);
                }
                else
                {
                    var fieldInfo = type.GetField(_variableName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (fieldInfo != null)
                    {
                        _getter = () => (int)fieldInfo.GetValue(_value);
                    }
                }
            }
        }
    }

    #endregion
    
}
