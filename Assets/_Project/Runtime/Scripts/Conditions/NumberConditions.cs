using System;
using UnityEngine;

namespace Metroma
{
    #region Float conditions

    [Serializable]
    public class FloatGreater : ICondition
    {
        [SerializeReference, ConditionAttribute(ConditionAttribute.ConditionType.MathsCalculation)] private IFloat a;
        [SerializeReference, ConditionAttribute(ConditionAttribute.ConditionType.MathsCalculation)] private IFloat b;

        public bool Evaluate()
        {
            return a.Evaluate() > b.Evaluate();
        }
    }

    [Serializable]
    public class FloatGreaterOrEqual : ICondition
    {
        [SerializeReference, ConditionAttribute(ConditionAttribute.ConditionType.MathsCalculation)] private IFloat a;
        [SerializeReference, ConditionAttribute(ConditionAttribute.ConditionType.MathsCalculation)] private IFloat b;

        public bool Evaluate()
        {
            return a.Evaluate() >= b.Evaluate();
        }
    }

    [Serializable]
    public class FloatLess : ICondition
    {
        [SerializeReference, ConditionAttribute(ConditionAttribute.ConditionType.MathsCalculation)] private IFloat a;
        [SerializeReference, ConditionAttribute(ConditionAttribute.ConditionType.MathsCalculation)] private IFloat b;

        public bool Evaluate()
        {
            return a.Evaluate() < b.Evaluate();
        }
    }

    [Serializable]
    public class FloatLessOrEqual : ICondition
    {
        [SerializeReference, ConditionAttribute(ConditionAttribute.ConditionType.MathsCalculation)] private IFloat a;
        [SerializeReference, ConditionAttribute(ConditionAttribute.ConditionType.MathsCalculation)] private IFloat b;

        public bool Evaluate()
        {
            return a.Evaluate() <= b.Evaluate();
        }
    }

    [Serializable]
    public class FloatEqual : ICondition
    {
        [SerializeReference, ConditionAttribute(ConditionAttribute.ConditionType.MathsCalculation)] private IFloat a;
        [SerializeReference, ConditionAttribute(ConditionAttribute.ConditionType.MathsCalculation)] private IFloat b;

        public bool Evaluate()
        {
            return Mathf.Approximately(a.Evaluate(), b.Evaluate());
        }
    }

    #endregion

    #region Int conditions

    [Serializable]
    public class IntGreater : ICondition
    {
        [SerializeReference, ConditionAttribute(ConditionAttribute.ConditionType.MathsCalculation)] private IInt a;
        [SerializeReference, ConditionAttribute(ConditionAttribute.ConditionType.MathsCalculation)] private IInt b;

        public bool Evaluate()
        {
            return a.Evaluate() > b.Evaluate();
        }
    }

    [Serializable]
    public class IntGreaterOrEqual : ICondition
    {
        [SerializeReference, ConditionAttribute(ConditionAttribute.ConditionType.MathsCalculation)] private IInt a;
        [SerializeReference, ConditionAttribute(ConditionAttribute.ConditionType.MathsCalculation)] private IInt b;

        public bool Evaluate()
        {
            return a.Evaluate() >= b.Evaluate();
        }
    }

    [Serializable]
    public class IntLess : ICondition
    {
        [SerializeReference, ConditionAttribute(ConditionAttribute.ConditionType.MathsCalculation)] private IInt a;
        [SerializeReference, ConditionAttribute(ConditionAttribute.ConditionType.MathsCalculation)] private IInt b;

        public bool Evaluate()
        {
            return a.Evaluate() < b.Evaluate();
        }
    }

    [Serializable]
    public class IntLessOrEqual : ICondition
    {
        [SerializeReference, ConditionAttribute(ConditionAttribute.ConditionType.MathsCalculation)] private IInt a;
        [SerializeReference, ConditionAttribute(ConditionAttribute.ConditionType.MathsCalculation)] private IInt b;

        public bool Evaluate()
        {
            return a.Evaluate() <= b.Evaluate();
        }
    }

    [Serializable]
    public class IntEqual : ICondition
    {
        [SerializeReference, ConditionAttribute(ConditionAttribute.ConditionType.MathsCalculation)] private IInt a;
        [SerializeReference, ConditionAttribute(ConditionAttribute.ConditionType.MathsCalculation)] private IInt b;

        public bool Evaluate()
        {
            return a.Evaluate() == b.Evaluate();
        }
    }

    #endregion
}