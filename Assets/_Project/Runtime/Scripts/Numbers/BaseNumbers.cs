using System;
using UnityEngine;

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
        
        private IFloat a;
        private IFloat b;
        
        public float Evaluate() {
            return a.Evaluate() + b.Evaluate();
        }
    }
    
    [Serializable]
    public class FloatMul : IFloat
    {
        private IFloat a;
        private IFloat b;
        
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
        
        private IInt a;
        private IInt b;
        
        public int Evaluate() {
            return a.Evaluate() + b.Evaluate();
        }
    }
    
    [Serializable]
    public class IntMul : IInt
    {
        private IInt a;
        private IInt b;
        
        public int Evaluate() {
            return a.Evaluate() * b.Evaluate();
        }
    }

    #endregion
    
}
