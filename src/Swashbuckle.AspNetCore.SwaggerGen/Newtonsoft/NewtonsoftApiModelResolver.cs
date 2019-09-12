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

            if (stringEnumConverter != null)
            {
                var enumValues = type.GetFields(BindingFlags.Public | BindingFlags.Static)
                    .Select(f =>
                    {
                        var enumMemberAttribute = f.GetCustomAttributes().OfType<EnumMemberAttribute>().FirstOrDefault();
                        return enumMemberAttribute?.Value ?? ResolveApiEnumValue(f.Name, stringEnumConverter);
                    })
                    .Distinct();

                return new ApiPrimitive(
                    type: type,
                    isStringEnum: true,
                    apiEnumValues: enumValues);
            }
            else
            {
                return new ApiPrimitive(
                    type: type,
                    isStringEnum: false,
                    apiEnumValues: type.GetEnumValues().Cast<object>());
            }
        }

#if NETCOREAPP3_0
        private string ResolveApiEnumValue(string enumValue, StringEnumConverter stringEnumConverter)
        {
            return stringEnumConverter.NamingStrategy?.GetPropertyName(enumValue, false) ?? enumValue;
        }
#else
        private string ResolveApiEnumValue(string enumValue, StringEnumConverter stringEnumConverter)
        {
            return (stringEnumConverter.CamelCaseText)
                ? new CamelCaseNamingStrategy().GetPropertyName(enumValue, false)
                : enumValue;
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
                    return new ApiProperty(
                        apiName: jsonProperty.PropertyName,
                        type: jsonProperty.PropertyType,
                        apiRequired: (jsonProperty.Required == Required.Always || jsonProperty.Required == Required.AllowNull),
                        apiNullable: (jsonProperty.Required == Required.AllowNull),
                        apiReadOnly: (jsonProperty.Readable && !jsonProperty.Writable),
                        apiWriteOnly: (jsonProperty.Writable && !jsonProperty.Readable),
                        memberInfo: jsonProperty.DeclaringType.GetMember(jsonProperty.UnderlyingName).FirstOrDefault());
                });

            return new ApiObject(
                type: underlyingType,
                apiProperties: apiProperties,
                additionalPropertiesType: jsonObjectContract.ExtensionDataValueType);
        }
    }
}
