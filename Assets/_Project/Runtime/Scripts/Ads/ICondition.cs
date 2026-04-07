using UnityEngine;

namespace Metroma
{
    public interface ICondition<T>
    {
        public bool Evaluate(T context);
    }
}
