using System;
using Cysharp.Threading.Tasks;

namespace DDEngine.Utils
{
    public static class UnitaskHelper
    {
        public static Action<T> Action<T>(Func<T, UniTaskVoid> func)
        {
            return arg => func(arg).Forget();
        }
    }
}
