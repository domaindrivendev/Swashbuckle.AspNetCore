using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Swashbuckle.AspNetCore.Newtonsoft
{
    public class NewtonsoftMetadataResolver : ISerializerMetadataResolver
    {
        private readonly SchemaGeneratorOptions _generatorOptions;
        private readonly JsonSerializerSettings _serializerSettings;
        private readonly IContractResolver _contractResolver;

        public NewtonsoftMetadataResolver(SchemaGeneratorOptions generatorOptions, JsonSerializerSettings serializerSettings)
        {
            _generatorOptions = generatorOptions;
            _serializerSettings = serializerSettings;
            _contractResolver = serializerSettings.ContractResolver ?? new DefaultContractResolver();
        }

        public SerializerMetadata GetSerializerMetadataForType(Type type)
        {
            var jsonContract = _contractResolver.ResolveContract(type.IsNullable(out Type innerType) ? innerType : type);

            if (jsonContract is JsonPrimitiveContract && !jsonContract.UnderlyingType.IsEnum)
            {
                var primitiveTypeAndFormat = PrimitiveTypesAndFormats.ContainsKey(jsonContract.UnderlyingType)
                    ? PrimitiveTypesAndFormats[jsonContract.UnderlyingType]
                    : Tuple.Create("string", (string)null);

                return SerializerMetadata.ForPrimitive(jsonContract.UnderlyingType, primitiveTypeAndFormat.Item1, primitiveTypeAndFormat.Item2);
            }

            if (jsonContract is JsonPrimitiveContract && jsonContract.UnderlyingType.IsEnum)
            {
                var enumValues = GetSerializerEnumValuesFor(jsonContract);

                var primitiveTypeAndFormat = (enumValues.Any(value => value.GetType() == typeof(string)))
                    ? PrimitiveTypesAndFormats[typeof(string)]
                    : PrimitiveTypesAndFormats[jsonContract.UnderlyingType.GetEnumUnderlyingType()];

                return SerializerMetadata.ForPrimitive(jsonContract.UnderlyingType, primitiveTypeAndFormat.Item1, primitiveTypeAndFormat.Item2, enumValues);
            }

            if (jsonContract is JsonDictionaryContract jsonDictionaryContract)
            {
                var keyType = jsonDictionaryContract.DictionaryKeyType ?? typeof(object);
                var valueType = jsonDictionaryContract.DictionaryValueType ?? typeof(object);

                return SerializerMetadata.ForDictionary(jsonDictionaryContract.UnderlyingType, keyType, valueType);
            }

            if (jsonContract is JsonArrayContract jsonArrayContract)
            {
                var itemType = jsonArrayContract.CollectionItemType ?? typeof(object);
                return SerializerMetadata.ForArray(jsonArrayContract.UnderlyingType, itemType);
            }

            if (jsonContract is JsonObjectContract jsonObjectContract)
            {
                return SerializerMetadata.ForObject(
                    jsonContract.UnderlyingType,
                    GetSerializerPropertiesFor(jsonObjectContract),
                    jsonObjectContract.ExtensionDataValueType);
            }

            if (jsonContract is JsonLinqContract jsonLinqContract)
            {
                if (jsonLinqContract.UnderlyingType == typeof(JArray))
                    return SerializerMetadata.ForArray(typeof(JArray), typeof(JToken));

                if (jsonLinqContract.UnderlyingType == typeof(JObject))
                    return SerializerMetadata.ForObject(typeof(JObject));
            }

            return SerializerMetadata.ForDynamic(jsonContract.UnderlyingType);
        }

        private IEnumerable<object> GetSerializerEnumValuesFor(JsonContract jsonContract)
        {
            var stringEnumConverter = (jsonContract.Converter as StringEnumConverter)
                ?? _serializerSettings.Converters.OfType<StringEnumConverter>().FirstOrDefault();

            // Temporary shim to support obsolete config options
            if (stringEnumConverter == null && _generatorOptions.DescribeAllEnumsAsStrings)
            {
                stringEnumConverter = new StringEnumConverter(_generatorOptions.DescribeStringEnumsInCamelCase);
            }
 
            if (stringEnumConverter != null)
            {
                return jsonContract.UnderlyingType.GetMembers(BindingFlags.Public | BindingFlags.Static)
                    .Select(member =>
                    {
                        var memberAttribute = member.GetCustomAttributes<EnumMemberAttribute>().FirstOrDefault();
                        return GetConvertedEnumName((memberAttribute?.Value ?? member.Name), (memberAttribute?.Value != null), stringEnumConverter);
                    })
                    .ToList();
            }

            return jsonContract.UnderlyingType.GetEnumValues()
                .Cast<object>();
        }

#if NETCOREAPP3_0
        private string GetConvertedEnumName(string enumName, bool hasSpecifiedName, StringEnumConverter stringEnumConverter)
        {
            if (stringEnumConverter.NamingStrategy != null)
                return stringEnumConverter.NamingStrategy.GetPropertyName(enumName, hasSpecifiedName);

            return (stringEnumConverter.CamelCaseText)
                ? new CamelCaseNamingStrategy().GetPropertyName(enumName, hasSpecifiedName)
                : enumName;
        }
#else
        private string GetConvertedEnumName(string enumName, bool hasSpecifiedName, StringEnumConverter stringEnumConverter)
        {
            return (stringEnumConverter.CamelCaseText)
                ? new CamelCaseNamingStrategy().GetPropertyName(enumName, hasSpecifiedName)
                : enumName;
        }
#endif

        private IEnumerable<SerializerPropertyMetadata> GetSerializerPropertiesFor(JsonObjectContract jsonObjectContract)
        {
            if (jsonObjectContract.UnderlyingType == typeof(object))
            {
                return null;
            }

            var serializerProperties = new List<SerializerPropertyMetadata>();

            foreach (var jsonProperty in jsonObjectContract.Properties)
            {
                if (jsonProperty.Ignored) continue;

                var required = jsonProperty.IsRequiredSpecified()
                    ? jsonProperty.Required
                    : jsonObjectContract.ItemRequired ?? Required.Default;

                jsonProperty.TryGetMemberInfo(out MemberInfo memberInfo);

                serializerProperties.Add(
                    new SerializerPropertyMetadata(
                        name: jsonProperty.PropertyName,
                        memberType: jsonProperty.PropertyType,
                        memberInfo: memberInfo,
                        isNullable: (required == Required.AllowNull || required == Required.Default) && jsonProperty.PropertyType.IsReferenceOrNullableType(),
                        isRequired: (required == Required.Always || required == Required.AllowNull)));
            }

            return serializerProperties;
        }

        private static readonly Dictionary<Type, Tuple<string, string>> PrimitiveTypesAndFormats = new Dictionary<Type, Tuple<string, string>>
        {
            [ typeof(bool) ] = Tuple.Create("boolean", (string)null),
            [ typeof(byte) ] = Tuple.Create("integer", "int32"),
            [ typeof(sbyte) ] = Tuple.Create("integer", "int32"),
            [ typeof(short) ] = Tuple.Create("integer", "int32"),
            [ typeof(ushort) ] = Tuple.Create("integer", "int32"),
            [ typeof(int) ] = Tuple.Create("integer", "int32"),
            [ typeof(uint) ] = Tuple.Create("integer", "int32"),
            [ typeof(long) ] = Tuple.Create("integer", "int64"),
            [ typeof(ulong) ] = Tuple.Create("integer", "int64"),
            [ typeof(float) ] = Tuple.Create("number", "float"),
            [ typeof(double) ] = Tuple.Create("number", "double"),
            [ typeof(decimal) ] = Tuple.Create("number", "double"),
            [ typeof(byte[]) ] = Tuple.Create("string", "byte"),
            [ typeof(string) ] = Tuple.Create("string", (string)null),
            [ typeof(char) ] = Tuple.Create("string", (string)null),
            [ typeof(DateTime) ] = Tuple.Create("string", "date-time"),
            [ typeof(DateTimeOffset) ] = Tuple.Create("string", "date-time"),
            [ typeof(Guid) ] = Tuple.Create("string", "uuid"),
            [ typeof(Uri) ] = Tuple.Create("string", "uri"),
            [ typeof(TimeSpan) ] = Tuple.Create("string", "date-span")
        };
    }
}
