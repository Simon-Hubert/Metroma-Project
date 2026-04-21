using System;
using UnityEngine;

namespace Metroma
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class ConditionAttribute : PropertyAttribute
    {
        public enum ConditionType
        {
            Decorator,
            MathsComparison,
            MathsCalculation
        }
        
        private ConditionType _conditionType;
        
        public ConditionAttribute(ConditionType conditionType)
        {
            _conditionType = conditionType;
        }
        
        private static Type[] DecoratorTypes = new[]
        {
            typeof(Nevers),
            typeof(Always),
            typeof(And),
            typeof(Or),
            typeof(Not),
            typeof(NumberComparison)
        };

        private static Type[] MathsCalculation = new[]
        {
            typeof(Float),
            typeof(FloatAdd),
            typeof(FloatMul),
            typeof(Int),
            typeof(IntAdd),
            typeof(IntMul)
        };
        
        private static Type[] MathsComparison = new[]
        {
            typeof(FloatGreater),
            typeof(FloatGreaterOrEqual),
            typeof(FloatEqual),
            typeof(FloatLess),
            typeof(FloatLessOrEqual),

            typeof(IntGreater),
            typeof(IntGreaterOrEqual),
            typeof(IntEqual),
            typeof(IntLess),
            typeof(IntLessOrEqual)
        };
        
        public Type[] GetTypes()
        {
            switch (_conditionType)
            {
                case ConditionType.Decorator:
                    return DecoratorTypes;
                case ConditionType.MathsComparison:
                    return MathsComparison;
                case ConditionType.MathsCalculation:
                    return MathsCalculation;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
