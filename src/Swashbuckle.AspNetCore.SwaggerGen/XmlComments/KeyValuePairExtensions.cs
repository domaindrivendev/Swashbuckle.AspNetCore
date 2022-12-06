using System.Collections.Generic;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public static class KeyValuePairExtensions
    {
        // Explicit deconstruct required for older .NET frameworks
        public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> kvp, out TKey key, out TValue value)
        {
            key = kvp.Key;
            value = kvp.Value;
        }
    }
}