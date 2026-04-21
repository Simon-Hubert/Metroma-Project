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
        
        [SerializeReference, ConditionAttribute(ConditionAttribute.ConditionType.MathsCalculation)] IFloat a;
        [SerializeReference, ConditionAttribute(ConditionAttribute.ConditionType.MathsCalculation)] IFloat b;
        
        public float Evaluate() {
            return a.Evaluate() + b.Evaluate();
        }
    }
    
    [Serializable]
    public class FloatMul : IFloat
    {
        [SerializeReference, ConditionAttribute(ConditionAttribute.ConditionType.MathsCalculation)] private IFloat a;
        [SerializeReference, ConditionAttribute(ConditionAttribute.ConditionType.MathsCalculation)] private IFloat b;
        
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
        
        [SerializeReference, ConditionAttribute(ConditionAttribute.ConditionType.MathsCalculation)] private IInt a;
        [SerializeReference, ConditionAttribute(ConditionAttribute.ConditionType.MathsCalculation)] private IInt b;
        
        public int Evaluate() {
            return a.Evaluate() + b.Evaluate();
        }
    }
    
    [Serializable]
    public class IntMul : IInt
    {
        [SerializeReference, ConditionAttribute(ConditionAttribute.ConditionType.MathsCalculation)] private IInt a;
        [SerializeReference, ConditionAttribute(ConditionAttribute.ConditionType.MathsCalculation)] private IInt b;
        
        public int Evaluate() {
            return a.Evaluate() * b.Evaluate();
        }
    }

    #endregion

    #region References
    
    public class FloatReference : IFloat
    {
        [SerializeField] private Object _value;

        private IFloatProvider _provider;
        private bool _isInitialized = false;
        
        public float Evaluate()
        {
            if (!_isInitialized)
            {
                _isInitialized = true;
                if (_value is GameObject g)
                {
                    _provider = g.GetComponent<IFloatProvider>();
                }

                IFloatProvider floatProvider = _value as IFloatProvider;
                if (floatProvider != null)
                {
                    _provider = floatProvider;
                }
            }
            
            if (_provider == null) return -1f;
            
            return _provider.GetFloatValue();
        }
    }
    
    public class IntReference : IInt
    {
        [SerializeField] private Object _value;

        private IIntProvider _provider;
        private bool _isInitialized = false;
        
        public int Evaluate()
        {
            if (!_isInitialized)
            {
                _isInitialized = true;
                if (_value is GameObject g)
                {
                    _provider = g.GetComponent<IIntProvider>();
                }

                IIntProvider floatProvider = _value as IIntProvider;
                if (floatProvider != null)
                {
                    _provider = floatProvider;
                }
            }
            
            if (_provider == null) return -1;
            
            return _provider.GetIntValue();
        }
    }

    #endregion
    
}
