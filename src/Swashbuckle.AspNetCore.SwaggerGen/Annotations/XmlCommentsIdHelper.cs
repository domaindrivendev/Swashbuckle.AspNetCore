using System;
using System.Reflection;
using System.Text;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public class XmlCommentsIdHelper
    {
        public static string GetCommentIdForMethod(MethodInfo methodInfo)
        {
            var builder = new StringBuilder("M:");
            AppendFullTypeName(methodInfo.DeclaringType, builder);
            builder.Append(".");
            AppendMethodName(methodInfo, builder);

            return builder.ToString();
        }

        public static string GetCommentIdForType(Type type)
        {
            var builder = new StringBuilder("T:");
            AppendFullTypeName(type, builder, expandGenericArgs: false);

            return builder.ToString();
        }

        public static string GetCommentIdForProperty(PropertyInfo propertyInfo)
        {
            var builder = new StringBuilder("P:");
            AppendFullTypeName(propertyInfo.DeclaringType, builder);
            builder.Append(".");
            AppendPropertyName(propertyInfo, builder);

            return builder.ToString();
        }

        private static void AppendFullTypeName(Type type, StringBuilder builder, bool expandGenericArgs = false)
        {
            if (type.Namespace != null)
            {
                builder.Append(type.Namespace);
                builder.Append(".");
            }
            AppendTypeName(type, builder, expandGenericArgs);
        }

        private static void AppendTypeName(Type type, StringBuilder builder, bool expandGenericArgs)
        {
            if (type.IsNested)
            {
                AppendTypeName(type.DeclaringType, builder, false);
                builder.Append(".");
            }

            builder.Append(type.Name);

            if (expandGenericArgs)
                ExpandGenericArgsIfAny(type, builder);
        }

        public static void ExpandGenericArgsIfAny(Type type, StringBuilder builder)
        {
            if (type.GetTypeInfo().IsGenericType)
            {
                var genericArgsBuilder = new StringBuilder("{");

                var genericArgs = type.GetGenericArguments();
                foreach (var argType in genericArgs)
                {
                    AppendFullTypeName(argType, genericArgsBuilder, true);
                    genericArgsBuilder.Append(",");
                }
                genericArgsBuilder.Replace(",", "}", genericArgsBuilder.Length - 1, 1);

                builder.Replace(string.Format("`{0}", genericArgs.Length), genericArgsBuilder.ToString());
            }
            else if (type.IsArray)
                ExpandGenericArgsIfAny(type.GetElementType(), builder);
        }

        private static void AppendMethodName(MethodInfo methodInfo, StringBuilder builder)
        {
            builder.Append(methodInfo.Name);

            var parameters = methodInfo.GetParameters();
            if (parameters.Length == 0) return;

            builder.Append("(");
            foreach (var param in parameters)
            {
                AppendFullTypeName(param.ParameterType, builder, true);
                builder.Append(",");
            }
            builder.Replace(",", ")", builder.Length - 1, 1);
        }

        private static void AppendPropertyName(PropertyInfo propertyInfo, StringBuilder builder)
        {
            builder.Append(propertyInfo.Name);
        }
    }
}
