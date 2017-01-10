using System;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public static class TypeExtensions
    {
        public static string FriendlyId(this Type type, bool fullyQualified = false)
        {
            var typeName = fullyQualified
                ? type.FullNameSansTypeParameters().Replace("+", ".")
                : type.Name;

            if (type.GetTypeInfo().IsGenericType)
            {
                var genericArgumentIds = type.GetGenericArguments()
                    .Select(t => t.FriendlyId(fullyQualified))
                    .ToArray();

                return new StringBuilder(typeName)
                    .Replace(string.Format("`{0}", genericArgumentIds.Count()), string.Empty)
                    .Append(string.Format("[{0}]", string.Join(",", genericArgumentIds).TrimEnd(',')))
                    .ToString();
            }

            return typeName;
        }

        internal static string FullNameSansTypeParameters(this Type type)
        {
            var fullName = type.FullName;
            var chopIndex = fullName.IndexOf("[[");
            return (chopIndex == -1) ? fullName : fullName.Substring(0, chopIndex);
        }

        internal static Type GetNonNullableType(this Type type)
        {
            if (!type.IsNullableType())
                return type;

            return type.GetGenericArguments()[0];
        }

        internal static bool IsNullableType(this Type type)
        {
            return Nullable.GetUnderlyingType(type) != null;
        }

        internal static bool IsGenericType(this PropertyInfo propInfo)
        {
            return IntrospectionExtensions.GetTypeInfo(propInfo.PropertyType).IsGenericType;
        }

        internal static bool IsEnumType(this Type type)
        {
            return IntrospectionExtensions.GetTypeInfo(type.GetNonNullableType()).IsEnum;
        }
    }
}
