using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public static class MemberInfoExtensions
    {
#if !NET6_0_OR_GREATER
        private const string NullableAttributeFullTypeName = "System.Runtime.CompilerServices.NullableAttribute";
        private const string NullableFlagsFieldName = "NullableFlags";
        private const string NullableContextAttributeFullTypeName = "System.Runtime.CompilerServices.NullableContextAttribute";
        private const string FlagFieldName = "Flag";
        private const int NotAnnotated = 1; // See https://github.com/dotnet/roslyn/blob/af7b0ebe2b0ed5c335a928626c25620566372dd1/docs/features/nullable-metadata.md?plain=1#L40
#endif

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

#if NET6_0_OR_GREATER
        private static NullabilityInfo GetNullabilityInfo(this MemberInfo memberInfo)
        {
            var context = new NullabilityInfoContext();

            return memberInfo switch
            {
                FieldInfo fieldInfo => context.Create(fieldInfo),
                PropertyInfo propertyInfo => context.Create(propertyInfo),
                EventInfo eventInfo => context.Create(eventInfo),
                _ => throw new InvalidOperationException($"MemberInfo type {memberInfo.MemberType} is not supported.")
            };
        }
#endif

        public static bool IsNonNullableReferenceType(this MemberInfo memberInfo)
        {
#if NET6_0_OR_GREATER
            var nullableInfo = GetNullabilityInfo(memberInfo);
            return nullableInfo.ReadState == NullabilityState.NotNull;
#else
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
                flags.Length >= 1 && flags[0] == NotAnnotated)
            {
                return true;
            }

            return false;
#endif
        }

        public static bool IsDictionaryValueNonNullable(this MemberInfo memberInfo)
        {
#if NET6_0_OR_GREATER
            var nullableInfo = GetNullabilityInfo(memberInfo);

            // Assume one generic argument means TKey and TValue are the same type.
            // Assume two generic arguments match TKey and TValue for a dictionary.
            // A better solution would be to inspect the type declaration (base types,
            // interfaces, etc.) to determine if the type is a dictionary, but the
            // nullability information is not available to be able to do that.
            // See https://stackoverflow.com/q/75786306/1064169.
            return nullableInfo.GenericTypeArguments.Length switch
            {
                1 => nullableInfo.GenericTypeArguments[0].ReadState == NullabilityState.NotNull,
                2 => nullableInfo.GenericTypeArguments[1].ReadState == NullabilityState.NotNull,
                _ => false,
            };
#else
            var memberType = memberInfo.MemberType == MemberTypes.Field
                ? ((FieldInfo)memberInfo).FieldType
                : ((PropertyInfo)memberInfo).PropertyType;

            if (memberType.IsValueType) return false;

            var nullableAttribute = memberInfo.GetNullableAttribute();
            var genericArguments = memberType.GetGenericArguments();

            if (genericArguments.Length != 2)
            {
                return false;
            }

            var valueArgument = genericArguments[1];
            var valueArgumentIsNullable = valueArgument.IsGenericType && valueArgument.GetGenericTypeDefinition() == typeof(Nullable<>);

            if (nullableAttribute == null)
            {
                return !valueArgumentIsNullable && memberInfo.GetNullableFallbackValue();
            }

            if (nullableAttribute.GetType().GetField(NullableFlagsFieldName) is FieldInfo field)
            {
                if (field.GetValue(nullableAttribute) is byte[] flags)
                {
                    // See https://github.com/dotnet/roslyn/blob/af7b0ebe2b0ed5c335a928626c25620566372dd1/docs/features/nullable-metadata.md
                    // Observations in the debugger show that the arity of the flags array is 3 only if all 3 items are reference types, i.e.
                    // Dictionary<string, object> would have arity 3 (one for the Dictionary, one for the string key, one for the object value),
                    // however Dictionary<string, int> would have arity 2 (one for the Dictionary, one for the string key), the value is skipped
                    // due it being a value type.
                    if (flags.Length == 2)  // Value in the dictionary is a value type.
                    {
                        return !valueArgumentIsNullable;
                    }
                    else if (flags.Length == 3) // Value in the dictionary is a reference type.
                    {
                        return flags[2] == NotAnnotated;
                    }
                }
            }

            return false;
#endif
        }

#if !NET6_0_OR_GREATER
        private static object GetNullableAttribute(this MemberInfo memberInfo)
        {
            var nullableAttribute = memberInfo
                .GetCustomAttributes()
                .FirstOrDefault(attr => string.Equals(attr.GetType().FullName, NullableAttributeFullTypeName));

            return nullableAttribute;
        }

        private static bool GetNullableFallbackValue(this MemberInfo memberInfo)
        {
            var declaringTypes = memberInfo.DeclaringType.IsNested
                ? GetDeclaringTypeChain(memberInfo)
                : [memberInfo.DeclaringType];

            foreach (var declaringType in declaringTypes)
            {
                IEnumerable<object> attributes = declaringType.GetCustomAttributes(false);

                var nullableContext = attributes
                    .FirstOrDefault(attr => string.Equals(attr.GetType().FullName, NullableContextAttributeFullTypeName));

                if (nullableContext != null)
                {
                    if (nullableContext.GetType().GetField(FlagFieldName) is FieldInfo field &&
                        field.GetValue(nullableContext) is byte flag && flag == NotAnnotated)
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
#endif

        private static List<Type> GetDeclaringTypeChain(MemberInfo memberInfo)
        {
            var chain = new List<Type>();
            var currentType = memberInfo.DeclaringType;

            while (currentType != null)
            {
                chain.Add(currentType);
                currentType = currentType.DeclaringType;
            }

            return chain;
        }
    }
}
