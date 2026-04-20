using System;
using UnityEngine;

namespace Metroma
{
    #region Float conditions

    [Serializable]
    public class FloatGreater : ICondition
    {
        [SerializeField] private IFloat a;
        [SerializeField] private IFloat b;

        public bool Evaluate()
        {
            return a.Evaluate() > b.Evaluate();
        }
    }

    [Serializable]
    public class FloatGreaterOrEqual : ICondition
    {
        [SerializeField] private IFloat a;
        [SerializeField] private IFloat b;

        public bool Evaluate()
        {
            return a.Evaluate() >= b.Evaluate();
        }
    }

    [Serializable]
    public class FloatLess : ICondition
    {
        [SerializeField] private IFloat a;
        [SerializeField] private IFloat b;

        public bool Evaluate()
        {
            return a.Evaluate() < b.Evaluate();
        }
    }

    [Serializable]
    public class FloatLessOrEqual : ICondition
    {
        [SerializeField] private IFloat a;
        [SerializeField] private IFloat b;

        public bool Evaluate()
        {
            return a.Evaluate() <= b.Evaluate();
        }
    }

    [Serializable]
    public class FloatEqual : ICondition
    {
        [SerializeField] private IFloat a;
        [SerializeField] private IFloat b;

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
        [SerializeField] private IInt a;
        [SerializeField] private IInt b;

        public bool Evaluate()
        {
            return a.Evaluate() > b.Evaluate();
        }
    }

    [Serializable]
    public class IntGreaterOrEqual : ICondition
    {
        [SerializeField] private IInt a;
        [SerializeField] private IInt b;

        public bool Evaluate()
        {
            return a.Evaluate() >= b.Evaluate();
        }
    }

    [Serializable]
    public class IntLess : ICondition
    {
        [SerializeField] private IInt a;
        [SerializeField] private IInt b;

        public bool Evaluate()
        {
            return a.Evaluate() < b.Evaluate();
        }
    }

    [Serializable]
    public class IntLessOrEqual : ICondition
    {
        [SerializeField] private IInt a;
        [SerializeField] private IInt b;

        public bool Evaluate()
        {
            return a.Evaluate() <= b.Evaluate();
        }
    }

    [Serializable]
    public class IntEqual : ICondition
    {
        [SerializeField] private IInt a;
        [SerializeField] private IInt b;

        public bool Evaluate()
        {
            return a.Evaluate() == b.Evaluate();
        }
    }

    #endregion
}