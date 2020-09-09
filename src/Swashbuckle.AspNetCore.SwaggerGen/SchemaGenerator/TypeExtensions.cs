using System;
using System.Collections.Generic;
using System.Linq;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public static class TypeExtensions
    {
        public static bool IsOneOf(this Type type, params Type[] possibleTypes)
        {
            return possibleTypes.Any(possibleType => possibleType == type);
        }

        public static bool IsAssignableTo(this Type type, Type baseType)
        {
            return baseType.IsAssignableFrom(type);
        }

        public static bool IsAssignableToOneOf(this Type type, params Type[] possibleBaseTypes)
        {
            return possibleBaseTypes.Any(possibleBaseType => possibleBaseType.IsAssignableFrom(type));
        }

        public static bool IsConstructedFrom(this Type type, Type genericType, out Type constructedType)
        {
            constructedType = new[] { type }
                .Union(type.GetInterfaces())
                .FirstOrDefault(i => i.IsConstructedGenericType && i.GetGenericTypeDefinition() == genericType);

            return (constructedType != null);
        }

        public static bool IsNullable(this Type type, out Type innerType)
        {
            innerType = type.IsConstructedFrom(typeof(Nullable<>), out Type constructedType)
                ? constructedType.GenericTypeArguments[0]
                : null;

            return (innerType != null);
        }

        public static bool IsReferenceOrNullableType(this Type type)
        {
            return (!type.IsValueType || type.IsNullable(out Type _));
        }

        public static bool IsSet(this Type type)
        {
            return type.IsConstructedFrom(typeof(ISet<>), out Type _);
        }

        public static object GetDefaultValue(this Type type)
        {
            return type.IsValueType
                ? Activator.CreateInstance(type)
                : null;
        }

        public static int GetDepthOfInheritance(this Type type)
        {
            var depth = 0;
            var current = type;

            while (current.BaseType != null)
            {
                depth++;
                current = current.BaseType;
            }

            return depth;
        }
    }
}
