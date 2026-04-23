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
            FloatCalculation,
            IntCalculation
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
            typeof(NumberComparison),
            typeof(CollisionCondition)
        };

        private static Type[] FloatCalculation = new[]
        {
            typeof(Float),
            typeof(FloatReference),
            typeof(FloatAdd),
            typeof(FloatMul)
        };
        
        private static Type[] IntCalculation = new[]
        {
            typeof(Int),
            typeof(IntReference),
            typeof(IntAdd),
            typeof(IntMul)
        };
        
        private static Type[] MathsComparison = new[]
        {
            typeof(IntGreater),
            typeof(IntGreaterOrEqual),
            typeof(IntEqual),
            typeof(IntLess),
            typeof(IntLessOrEqual),
                
            typeof(FloatGreater),
            typeof(FloatGreaterOrEqual),
            typeof(FloatEqual),
            typeof(FloatLess),
            typeof(FloatLessOrEqual)
        };
        
        
        public Type[] GetTypes()
        {
            switch (_conditionType)
            {
                case ConditionType.Decorator:
                    return DecoratorTypes;
                case ConditionType.MathsComparison:
                    return MathsComparison;
                case ConditionType.FloatCalculation:
                    return FloatCalculation;
                case ConditionType.IntCalculation:
                    return IntCalculation;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
