using System;
using System.Collections.Generic;
using System.Reflection;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public interface IApiModelResolver
    {
        ApiModel ResolveApiModelFor(Type type);
    }

    public class ApiModel
    {
        public ApiModel(Type type)
        {
            Type = type;
        }

        public Type Type { get; }
    }

    public class ApiPrimitive : ApiModel
    {
        public ApiPrimitive(Type type)
            : base(type)
        { }

        public ApiPrimitive(Type type, bool isStringEnum, IEnumerable<object> apiEnumValues)
            : base(type)
        {
            IsEnum = true;
            IsStringEnum = isStringEnum;
            ApiEnumValues = apiEnumValues;
        }

        public bool IsEnum { get; }
        public bool IsStringEnum { get; }
        public IEnumerable<object> ApiEnumValues { get; }
    }

    public class ApiDictionary : ApiModel
    {
        public ApiDictionary(Type type, Type keyType, Type valueType)
            : base(type)
        {
            DictionaryKeyType = keyType;
            DictionaryValueType = valueType;
        }

        public Type DictionaryKeyType { get; }
        public Type DictionaryValueType { get; }
    }

    public class ApiArray : ApiModel
    {
        public ApiArray(Type type, Type itemType)
            : base(type)
        {
            ArrayItemType = itemType;
        }

        public Type ArrayItemType { get; }
    }

    public class ApiObject : ApiModel
    {
        public ApiObject(Type type, IEnumerable<ApiProperty> apiProperties, Type additionalPropertiesType = null)
            : base(type)
        {
            ApiProperties = apiProperties;
            AdditionalPropertiesType = additionalPropertiesType;
        }

        public IEnumerable<ApiProperty> ApiProperties { get; }
        public Type AdditionalPropertiesType { get; }
    }

    public class ApiProperty
    {
        public ApiProperty(
            Type type,
            string apiName,
            bool apiRequired,
            bool apiNullable,
            bool apiReadOnly,
            bool apiWriteOnly,
            MemberInfo memberInfo = null)
        {
            Type = type;
            ApiName = apiName;
            ApiRequired = apiRequired;
            ApiNullable = apiNullable;
            ApiReadOnly = apiReadOnly;
            ApiWriteOnly = apiWriteOnly;
            MemberInfo = memberInfo;
        }

        public Type Type { get; }
        public string ApiName { get; }
        public bool ApiRequired { get; }
        public bool ApiNullable { get; }
        public bool ApiReadOnly { get; }
        public bool ApiWriteOnly { get; }
        public MemberInfo MemberInfo { get; }
    }
}