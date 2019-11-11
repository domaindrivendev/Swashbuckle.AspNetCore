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
        private readonly SchemaGeneratorOptions _options;

#if NETCOREAPP3_0
        public NewtonsoftApiModelResolver(
            IOptions<MvcNewtonsoftJsonOptions> jsonOptionsAccessor,
            IOptions<SchemaGeneratorOptions> optionsAccessor)
            : this(
                jsonOptionsAccessor.Value?.SerializerSettings ?? new JsonSerializerSettings(),
                optionsAccessor.Value ?? new SchemaGeneratorOptions())
        { }
#else
        public NewtonsoftApiModelResolver(
            IOptions<MvcJsonOptions> jsonOptionsAccessor,
            IOptions<SchemaGeneratorOptions> optionsAccessor)
            : this(
                jsonOptionsAccessor.Value?.SerializerSettings ?? new JsonSerializerSettings(),
                optionsAccessor.Value ?? new SchemaGeneratorOptions())
        { }
#endif

        public NewtonsoftApiModelResolver(JsonSerializerSettings jsonSerializerSettings, SchemaGeneratorOptions options)
        {
            _jsonSerializerSettings = jsonSerializerSettings;
            _jsonContractResolver = jsonSerializerSettings.ContractResolver ?? new DefaultContractResolver();
            _options = options;
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

            // Temporary shim to support obsolete config options
            if (stringEnumConverter == null && _options.DescribeAllEnumsAsStrings)
            {
                stringEnumConverter = new StringEnumConverter(_options.DescribeStringEnumsInCamelCase);
            };

            if (stringEnumConverter == null)
            {
                return new ApiPrimitive(
                    type: type,
                    isStringEnum: false,
                    apiEnumValues: type.GetEnumValues().Cast<object>());
            }

            var enumValues = type.GetMembers(BindingFlags.Public | BindingFlags.Static)
                .Select(enumMember =>
                {
                    var enumMemberAttribute = enumMember.GetCustomAttributes<EnumMemberAttribute>().FirstOrDefault();
                    return GetConvertedEnumName(stringEnumConverter, (enumMemberAttribute?.Value ?? enumMember.Name), (enumMemberAttribute?.Value != null));
                })
                .Distinct();

            return new ApiPrimitive(
                type: type,
                isStringEnum: true,
                apiEnumValues: enumValues);
        }

#if NETCOREAPP3_0
        private string GetConvertedEnumName(StringEnumConverter stringEnumConverter, string enumName, bool hasSpecifiedName)
        {
            if (stringEnumConverter.NamingStrategy != null)
                return stringEnumConverter.NamingStrategy.GetPropertyName(enumName, hasSpecifiedName);

            return (stringEnumConverter.CamelCaseText)
                ? new CamelCaseNamingStrategy().GetPropertyName(enumName, hasSpecifiedName)
                : enumName;
        }
#else
        private string GetConvertedEnumName(StringEnumConverter stringEnumConverter, string enumName, bool hasSpecifiedName)
        {
            return (stringEnumConverter.CamelCaseText)
                ? new CamelCaseNamingStrategy().GetPropertyName(enumName, hasSpecifiedName)
                : enumName;
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
                    var jsonPropertyAttributeData = memberInfo?.GetCustomAttributesData()
                        .FirstOrDefault(attrData => attrData.AttributeType == typeof(JsonPropertyAttribute));

                    var required = (jsonPropertyAttributeData == null || !jsonPropertyAttributeData.NamedArguments.Any(arg => arg.MemberName == "Required"))
                        ? jsonObjectContract.ItemRequired
                        : jsonProperty.Required;

                    return new ApiProperty(
                        apiName: jsonProperty.PropertyName,
                        type: jsonProperty.PropertyType,
                        apiRequired: (required == Required.Always || required == Required.AllowNull),
                        apiNullable: (required != Required.Always && required != Required.DisallowNull && jsonProperty.PropertyType.IsReferenceOrNullableType()),
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
