using System;
using System.Collections.Generic;
using System.Linq;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public static class TypeExtensions
    {
        public static bool IsNullable(this Type type)
        {
            return type.FullName.StartsWith("System.Nullable`1");
        }

        public static bool IsFSharpOption(this Type type)
        {
            return type.FullName.StartsWith("Microsoft.FSharp.Core.FSharpOption`1");
        }

        public static bool IsSet(this Type type)
        {
            return new[] { type }
                .Union(type.GetInterfaces())
                .Any(i => i.IsConstructedGenericType && i.GetGenericTypeDefinition() == typeof(ISet<>));
        }
    }
}
