using System;
using System.Collections.Generic;
using System.Linq;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public static class TypeExtensions
    {
        public static bool IsSet(this Type type)
        {
            return type.IsConstructedFrom(typeof(ISet<>), out Type _);
        }

        public static bool IsNullable(this Type type, out Type valueType)
        {
            valueType = type.IsConstructedFrom(typeof(Nullable<>), out Type constructedType)
                ? constructedType.GenericTypeArguments[0]
                : null;

            return (valueType != null);
        }

        public static bool IsReferenceOrNullableType(this Type type)
        {
            return (!type.IsValueType || type.IsNullable(out Type _));
        }

        private static bool IsConstructedFrom(this Type type, Type genericType, out Type constructedType)
        {
            constructedType = new[] { type }
                .Union(type.GetInterfaces())
                .FirstOrDefault(i => i.IsConstructedGenericType && i.GetGenericTypeDefinition() == genericType);

            return (constructedType != null);
        }
    }
}
