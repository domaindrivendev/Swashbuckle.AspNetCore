using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public static class MemberInfoExtensions
    {
        private const string NullableAttributeFullTypeName = "System.Runtime.CompilerServices.NullableAttribute";
        private const string NullableFlagsFieldName = "NullableFlags";
        private const string NullableContextAttributeFullTypeName = "System.Runtime.CompilerServices.NullableContextAttribute";
        private const string FlagFieldName = "Flag";

        public static IEnumerable<object> GetInlineAndMetadataAttributes(this MemberInfo memberInfo)
        {
            var attributes = memberInfo.GetCustomAttributes(true)
                .ToList();

            var metadataTypeAttribute = memberInfo.DeclaringType.GetCustomAttributes(true)
                .OfType<ModelMetadataTypeAttribute>()
                .FirstOrDefault();

            var metadataMemberInfo = metadataTypeAttribute?.MetadataType.GetMember(memberInfo.Name)
                .FirstOrDefault();

            if (metadataMemberInfo != null)
            {
                attributes.AddRange(metadataMemberInfo.GetCustomAttributes(true));
            }

            return attributes;
        }

        public static bool IsNonNullableReferenceType(this MemberInfo memberInfo)
        {
            var memberType = memberInfo.MemberType == MemberTypes.Field
                ? ((FieldInfo)memberInfo).FieldType
                : ((PropertyInfo)memberInfo).PropertyType;

            if (memberType.IsValueType) return false;

            var nullableAttribute = memberInfo.GetNullableAttribute();

            if (nullableAttribute == null)
            {
                return memberInfo.GetNullableFallbackValue();
            }

            if (nullableAttribute.GetType().GetField(NullableFlagsFieldName) is FieldInfo field &&
                field.GetValue(nullableAttribute) is byte[] flags &&
                flags.Length >= 1 && flags[0] == 1)
            {
                return true;
            }

            return false;
        }

        private static object GetNullableAttribute(this MemberInfo memberInfo)
        {
            var nullableAttribute = memberInfo.GetCustomAttributes()
                .Where(attr => string.Equals(attr.GetType().FullName, NullableAttributeFullTypeName))
                .FirstOrDefault();

            return nullableAttribute;
        }

        private static bool GetNullableFallbackValue(this MemberInfo memberInfo)
        {
            var declaringTypes = memberInfo.DeclaringType.IsNested
                ? new Type[] { memberInfo.DeclaringType, memberInfo.DeclaringType.DeclaringType }
                : new Type[] { memberInfo.DeclaringType };

            foreach (var declaringType in declaringTypes)
            {
                var attributes = (IEnumerable<object>)declaringType.GetCustomAttributes(false);

                var nullableContext = attributes
                .Where(attr => string.Equals(attr.GetType().FullName, NullableContextAttributeFullTypeName))
                .FirstOrDefault();

                if (nullableContext != null)
                {
                    if (nullableContext.GetType().GetField(FlagFieldName) is FieldInfo field &&
                    field.GetValue(nullableContext) is byte flag && flag == 1)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return false;
        }
    }
}
