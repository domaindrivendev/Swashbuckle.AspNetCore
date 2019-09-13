using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Swashbuckle.AspNetCore.Newtonsoft
{
    public class NewtonsoftApiModelResolver : IApiModelResolver
    {
        private readonly JsonSerializerSettings _jsonSerializerSettings;
        private readonly IContractResolver _jsonContractResolver;

#if NETCOREAPP3_0
        public NewtonsoftApiModelResolver(IOptions<MvcNewtonsoftJsonOptions> jsonOptionsAccessor)
            : this(jsonOptionsAccessor.Value?.SerializerSettings ?? new JsonSerializerSettings())
        { }
#else
        public NewtonsoftApiModelResolver(IOptions<MvcJsonOptions> jsonOptionsAccessor)
            : this(jsonOptionsAccessor.Value?.SerializerSettings ?? new JsonSerializerSettings())
        { }
#endif

        public NewtonsoftApiModelResolver(JsonSerializerSettings jsonSerializerSettings)
        {
            _jsonSerializerSettings = jsonSerializerSettings;
            _jsonContractResolver = jsonSerializerSettings.ContractResolver ?? new DefaultContractResolver();
        }

        public ApiModel ResolveApiModelFor(Type type)
        {
            // If it's nullable, use the inner value type
            var underlyingType = type.IsNullable(out Type valueType)
                ? valueType
                : type;

            var jsonContract = _jsonContractResolver.ResolveContract(underlyingType);

            if (jsonContract is JsonPrimitiveContract jsonPrimitiveContract)
                return ResolveApiPrimitive(jsonPrimitiveContract);

            if (jsonContract is JsonDictionaryContract jsonDictionaryContract)
                return ResolveApiDictionary(jsonDictionaryContract);

            if (jsonContract is JsonArrayContract jsonArrayContract)
                return ResolveApiArray(jsonArrayContract);

            if (jsonContract is JsonObjectContract jsonObjectContract)
                return ResolveApiObject(jsonObjectContract);

            return new ApiModel(jsonContract.UnderlyingType);
        }

        private ApiModel ResolveApiPrimitive(JsonPrimitiveContract jsonPrimitiveContract)
        {
            var type = jsonPrimitiveContract.UnderlyingType;

            if (!type.IsEnum)
            {
                return new ApiPrimitive(type);
            }

            var stringEnumConverter = (jsonPrimitiveContract.Converter as StringEnumConverter)
                ?? _jsonSerializerSettings.Converters.OfType<StringEnumConverter>().FirstOrDefault();

            if (stringEnumConverter == null)
            {
                return new ApiPrimitive(
                    type: type,
                    isStringEnum: false,
                    apiEnumValues: type.GetEnumValues().Cast<object>());
            }

            var enumValues = type.GetFields(BindingFlags.Public | BindingFlags.Static)
                .Select(field =>
                {
                    var enumMemberAttribute = field.GetCustomAttributes<EnumMemberAttribute>().FirstOrDefault();
                    var memberName = enumMemberAttribute?.Value ?? field.Name;
                    return GetConvertedEnumName(memberName, stringEnumConverter);
                })
                .Distinct();

            return new ApiPrimitive(
                type: type,
                isStringEnum: true,
                apiEnumValues: enumValues);
        }

#if NETCOREAPP3_0
        private string GetConvertedEnumName(string memberName, StringEnumConverter stringEnumConverter)
        {
            return stringEnumConverter.NamingStrategy?.GetPropertyName(memberName, false) ?? memberName;
        }
#else
        private string GetConvertedEnumName(string memberName, StringEnumConverter stringEnumConverter)
        {
            return (stringEnumConverter.CamelCaseText)
                ? new CamelCaseNamingStrategy().GetPropertyName(memberName, false)
                : memberName;
        }
#endif

        private ApiModel ResolveApiDictionary(JsonDictionaryContract jsonDictionaryContract)
        {
            return new ApiDictionary(
                type: jsonDictionaryContract.UnderlyingType,
                keyType: jsonDictionaryContract.DictionaryKeyType ?? typeof(object),
                valueType: jsonDictionaryContract.DictionaryValueType ?? typeof(object));
        }

        private ApiModel ResolveApiArray(JsonArrayContract jsonArrayContract)
        {
            return new ApiArray(
                type: jsonArrayContract.UnderlyingType,
                itemType: jsonArrayContract.CollectionItemType ?? typeof(object));
        }

        private ApiModel ResolveApiObject(JsonObjectContract jsonObjectContract)
        {
            var underlyingType = jsonObjectContract.UnderlyingType;

            if (underlyingType == typeof(object))
            {
                return new ApiObject(
                    type: underlyingType,
                    apiProperties: Enumerable.Empty<ApiProperty>());
            }

            var apiProperties = jsonObjectContract.Properties
                .Where(jsonProperty => !jsonProperty.Ignored)
                .Select(jsonProperty =>
                {
                    var memberInfo = jsonProperty.DeclaringType.GetMember(jsonProperty.UnderlyingName).FirstOrDefault();
                    var jsonPropertyAttribute = memberInfo?.GetCustomAttributes<JsonPropertyAttribute>(true).FirstOrDefault();

                    var effectiveRequired = (jsonPropertyAttribute == null)
                        ? jsonObjectContract.ItemRequired
                        : jsonProperty.Required;

                    return new ApiProperty(
                        apiName: jsonProperty.PropertyName,
                        type: jsonProperty.PropertyType,
                        apiRequired: (effectiveRequired == Required.Always || effectiveRequired == Required.AllowNull),
                        apiNullable: (effectiveRequired != Required.Always && effectiveRequired != Required.DisallowNull && !jsonProperty.PropertyType.IsValueType),
                        apiReadOnly: (jsonProperty.Readable && !jsonProperty.Writable),
                        apiWriteOnly: (jsonProperty.Writable && !jsonProperty.Readable),
                        memberInfo: memberInfo);
                });

            return new ApiObject(
                type: underlyingType,
                apiProperties: apiProperties,
                additionalPropertiesType: jsonObjectContract.ExtensionDataValueType);
        }
    }
}
