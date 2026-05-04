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
        [SerializeReference, ConditionAttribute(ConditionAttribute.ConditionType.Decorator)] private ICondition _a;
        [SerializeReference, ConditionAttribute(ConditionAttribute.ConditionType.Decorator)] private ICondition _b;

        public bool Evaluate() {
            return _a.Evaluate() && _b.Evaluate();
        }
    }
    
    [Serializable]
    public class Or : ICondition
    {
        [SerializeReference, ConditionAttribute(ConditionAttribute.ConditionType.Decorator)] private ICondition _a;
        [SerializeReference, ConditionAttribute(ConditionAttribute.ConditionType.Decorator)] private ICondition _b;

        public bool Evaluate() {
            return _a.Evaluate() || _b.Evaluate();
        }
    }
    
    [Serializable]
    public class Not : ICondition
    {
        [SerializeReference, ConditionAttribute(ConditionAttribute.ConditionType.Decorator)] private ICondition _a;

        public bool Evaluate() {
            return !_a.Evaluate();
        }
    }

    [Serializable]
    public class NumberComparison : ICondition
    {
        [SerializeReference, ConditionAttribute(ConditionAttribute.ConditionType.MathsComparison)] private ICondition _a;

        public bool Evaluate() {
            return _a.Evaluate();
        }
    }

    [Serializable]
    public class Nevers : ICondition //haha c'est rigolo
    {
        public bool Evaluate()
        {
            return false;
        }
    }
    
    [Serializable]
    public class Always : ICondition
    {
        public bool Evaluate()
        {
            return true;
        }
    }
}
