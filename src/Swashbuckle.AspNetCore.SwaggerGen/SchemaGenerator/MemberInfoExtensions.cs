using System.Reflection;
using Microsoft.AspNetCore.Mvc;

namespace Swashbuckle.AspNetCore.SwaggerGen;

public static class MemberInfoExtensions
{
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

    public static bool IsNonNullableReferenceType(this MemberInfo memberInfo)
    {
        var nullableInfo = GetNullabilityInfo(memberInfo);
        return nullableInfo.ReadState == NullabilityState.NotNull;
    }

    public static bool IsDictionaryValueNonNullable(this MemberInfo memberInfo)
    {
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
    }
}
