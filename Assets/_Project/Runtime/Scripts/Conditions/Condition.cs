using System;
using UnityEngine;

namespace Metroma
{
    public interface ICondition
    {
        public bool Evaluate();
    }
    
    [Serializable]
    public class And : ICondition
    {
        private ICondition _a;
        private ICondition _b;

        public bool Evaluate() {
            return _a.Evaluate() && _b.Evaluate();
        }
    }
    
    [Serializable]
    public class Or : ICondition
    {
        private ICondition _a;
        private ICondition _b;

        public bool Evaluate() {
            return _a.Evaluate() || _b.Evaluate();
        }
    }
    
    [Serializable]
    public class Not : ICondition
    {
        private ICondition _a;

        public bool Evaluate() {
            return !_a.Evaluate();
        }
    }
}
