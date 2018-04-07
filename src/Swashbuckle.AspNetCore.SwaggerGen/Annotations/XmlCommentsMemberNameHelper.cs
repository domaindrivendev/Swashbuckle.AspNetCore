using System;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public class XmlCommentsMemberNameHelper
    {
        public static string GetMemberNameForMethod(MethodInfo method)
        {
            var builder = new StringBuilder("M:");

            // If method is from a constructed generic type, use the generic type
            var sourceMethod = method.DeclaringType.IsConstructedGenericType
                ? method.DeclaringType.GetGenericTypeDefinition().GetMethod(method.Name)
                : method;

            builder.Append(sourceMethod.DeclaringType.FullNameSansTypeArguments().Replace("+", "."));
            builder.Append($".{sourceMethod.Name}");

            var parameters = sourceMethod.GetParameters();
            if (parameters.Any())
            {
                var tokens = parameters.Select(p =>
                {
                    return p.ParameterType.IsGenericParameter
                        ? $"`{p.ParameterType.GenericParameterPosition}"
                        : BuildTypeToken(p.ParameterType);
                });
                builder.Append($"({string.Join(",", tokens)})");
            }

            return builder.ToString();
        }

        public static string GetMemberNameForType(Type type)
        {
            var builder = new StringBuilder("T:");
            builder.Append(type.FullNameSansTypeArguments().Replace("+", "."));

            return builder.ToString();
        }

        public static string GetMemberNameForMember(MemberInfo memberInfo)
        {
            var builder = new StringBuilder(((memberInfo.MemberType & MemberTypes.Field) != 0) ? "F:" : "P:");
            builder.Append(memberInfo.DeclaringType.FullNameSansTypeArguments().Replace("+", "."));
            builder.Append($".{memberInfo.Name}");

            return builder.ToString();
        }

        private static string BuildTypeToken(Type type)
        {
            if (type.IsArray)
                return $"{BuildTypeToken(type.GetElementType())}[]";

            var builder = new StringBuilder(type.FullNameSansTypeArguments().Replace("+", "."));

            var genericArgs = type.GetGenericArguments();
            if (genericArgs.Any())
            {
                var tokens = genericArgs.Select(t => BuildTypeToken(t));
                builder.Replace($"`{genericArgs.Count()}", $"{{{string.Join(",", tokens)}}}");
            }

            return builder.ToString();
        }
    }
}
