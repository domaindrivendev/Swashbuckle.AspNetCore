using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Swashbuckle.AspNetCore.Newtonsoft;

/// <summary>
/// A class representing an <see cref="ISerializerDataContractResolver"/> implementation that uses Newtonsoft.Json.
/// </summary>
/// <param name="serializerSettings">The <see cref="JsonSerializerSettings"/> to use.</param>
public class NewtonsoftDataContractResolver(JsonSerializerSettings serializerSettings) : ISerializerDataContractResolver
{
    private readonly JsonSerializerSettings _serializerSettings = serializerSettings;
    private readonly IContractResolver _contractResolver = serializerSettings.ContractResolver ?? new DefaultContractResolver();

    /// <inheritdoc/>
    public DataContract GetDataContractForType(Type type)
    {
        var effectiveType = Nullable.GetUnderlyingType(type) ?? type;
        if (effectiveType.IsOneOf(typeof(object), typeof(JToken), typeof(JObject), typeof(JArray)))
        {
            return DataContract.ForDynamic(
                underlyingType: effectiveType,
                jsonConverter: JsonConverterFunc);
        }

        var jsonContract = _contractResolver.ResolveContract(effectiveType);

        if (jsonContract is JsonPrimitiveContract && !jsonContract.UnderlyingType.IsEnum)
        {
            if (!PrimitiveTypesAndFormats.TryGetValue(jsonContract.UnderlyingType, out var primitiveTypeAndFormat))
            {
                primitiveTypeAndFormat = Tuple.Create(DataType.String, (string)null);
            }

            return DataContract.ForPrimitive(
                underlyingType: jsonContract.UnderlyingType,
                dataType: primitiveTypeAndFormat.Item1,
                dataFormat: primitiveTypeAndFormat.Item2,
                jsonConverter: JsonConverterFunc);
        }

        if (jsonContract is JsonPrimitiveContract && jsonContract.UnderlyingType.IsEnum)
        {
            var enumValues = jsonContract.UnderlyingType.GetEnumValues();

            // Test to determine if the serializer will treat as string
            var serializeAsString = (enumValues.Length > 0) && JsonConverterFunc(enumValues.GetValue(0)).StartsWith('\"');

            var primitiveTypeAndFormat = serializeAsString
                ? PrimitiveTypesAndFormats[typeof(string)]
                : PrimitiveTypesAndFormats[jsonContract.UnderlyingType.GetEnumUnderlyingType()];

            return DataContract.ForPrimitive(
                underlyingType: jsonContract.UnderlyingType,
                dataType: primitiveTypeAndFormat.Item1,
                dataFormat: primitiveTypeAndFormat.Item2,
                jsonConverter: JsonConverterFunc);
        }

        if (jsonContract is JsonArrayContract jsonArrayContract)
        {
            return DataContract.ForArray(
                underlyingType: jsonArrayContract.UnderlyingType,
                itemType: jsonArrayContract.CollectionItemType ?? typeof(object),
                jsonConverter: JsonConverterFunc);
        }

        if (jsonContract is JsonDictionaryContract jsonDictionaryContract)
        {
            var keyType = jsonDictionaryContract.DictionaryKeyType ?? typeof(object);
            var valueType = jsonDictionaryContract.DictionaryValueType ?? typeof(object);

            IEnumerable<string> keys = null;

            if (keyType.IsEnum)
            {
                // This is a special case where we know the possible key values
                var enumValuesAsJson = keyType.GetEnumValues()
                    .Cast<object>()
                    .Select(JsonConverterFunc);

                keys =
                    enumValuesAsJson.Any(json => json.StartsWith('\"'))
                    ? enumValuesAsJson.Select(json => json.Replace("\"", string.Empty))
                    : keyType.GetEnumNames();
            }

            return DataContract.ForDictionary(
                underlyingType: jsonDictionaryContract.UnderlyingType,
                valueType: valueType,
                keys: keys,
                jsonConverter: JsonConverterFunc);
        }

        if (jsonContract is JsonObjectContract jsonObjectContract)
        {
            // This handles DateOnly and TimeOnly
            if (PrimitiveTypesAndFormats.TryGetValue(jsonContract.UnderlyingType, out var primitiveTypeAndFormat))
            {
                return DataContract.ForPrimitive(
                    underlyingType: jsonContract.UnderlyingType,
                    dataType: primitiveTypeAndFormat.Item1,
                    dataFormat: primitiveTypeAndFormat.Item2,
                    jsonConverter: JsonConverterFunc);
            }

            string typeNameProperty = null;
            string typeNameValue = null;

            if (_serializerSettings.TypeNameHandling == TypeNameHandling.Objects ||
                _serializerSettings.TypeNameHandling == TypeNameHandling.All ||
                _serializerSettings.TypeNameHandling == TypeNameHandling.Auto)
            {
                typeNameProperty = "$type";

                typeNameValue = (_serializerSettings.TypeNameAssemblyFormatHandling == TypeNameAssemblyFormatHandling.Full)
                    ? jsonObjectContract.UnderlyingType.AssemblyQualifiedName
                    : $"{jsonObjectContract.UnderlyingType.FullName}, {jsonObjectContract.UnderlyingType.Assembly.GetName().Name}";
            }

            return DataContract.ForObject(
                underlyingType: jsonObjectContract.UnderlyingType,
                properties: GetDataPropertiesFor(jsonObjectContract, out Type extensionDataType),
                extensionDataType: extensionDataType,
                typeNameProperty: typeNameProperty,
                typeNameValue: typeNameValue,
                jsonConverter: JsonConverterFunc);
        }

        return DataContract.ForDynamic(
            underlyingType: effectiveType,
            jsonConverter: JsonConverterFunc);
    }

    private string JsonConverterFunc(object value)
        => JsonConvert.SerializeObject(value, _serializerSettings);

    private List<DataProperty> GetDataPropertiesFor(JsonObjectContract jsonObjectContract, out Type extensionDataType)
    {
        var dataProperties = new List<DataProperty>();

        foreach (var jsonProperty in jsonObjectContract.Properties)
        {
            bool memberInfoIsObtained = jsonProperty.TryGetMemberInfo(out MemberInfo memberInfo);
            if (jsonProperty.Ignored || (memberInfoIsObtained && memberInfo.CustomAttributes.Any(t => t.AttributeType == typeof(SwaggerIgnoreAttribute))))
            {
                continue;
            }
            var required = jsonProperty.IsRequiredSpecified()
                ? jsonProperty.Required
                : jsonObjectContract.ItemRequired ?? Required.Default;

            var isSetViaConstructor = jsonProperty.DeclaringType != null && jsonProperty.DeclaringType.GetConstructors()
                .SelectMany(c => c.GetParameters())
                .Any(p =>
                {
                    // Newtonsoft supports setting via constructor if either underlying OR JSON names match
                    return
                        string.Equals(p.Name, jsonProperty.UnderlyingName, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(p.Name, jsonProperty.PropertyName, StringComparison.OrdinalIgnoreCase);
                });

            dataProperties.Add(
                new DataProperty(
                    name: jsonProperty.PropertyName,
                    isRequired: required == Required.Always || required == Required.AllowNull,
                    isNullable: (required == Required.AllowNull || required == Required.Default) && jsonProperty.PropertyType.IsReferenceOrNullableType(),
                    isReadOnly: jsonProperty.Readable && !jsonProperty.Writable && !isSetViaConstructor,
                    isWriteOnly: jsonProperty.Writable && !jsonProperty.Readable,
                    memberType: jsonProperty.PropertyType,
                    memberInfo: memberInfo));
        }

        extensionDataType = jsonObjectContract.ExtensionDataValueType;

        // If applicable, honor ProblemDetailsConverter
        if (jsonObjectContract.UnderlyingType.IsAssignableTo(typeof(Microsoft.AspNetCore.Mvc.ProblemDetails))
            && _serializerSettings.Converters.OfType<Microsoft.AspNetCore.Mvc.NewtonsoftJson.ProblemDetailsConverter>().Any())
        {
            var extensionsProperty = dataProperties.FirstOrDefault(p => p.MemberInfo.Name == "Extensions");
            if (extensionsProperty != null)
            {
                dataProperties.Remove(extensionsProperty);
                extensionDataType = typeof(object);
            }
        }

        return dataProperties;
    }

    private static readonly Dictionary<Type, Tuple<DataType, string>> PrimitiveTypesAndFormats = new()
    {
        [typeof(bool)] = Tuple.Create(DataType.Boolean, (string)null),
        [typeof(byte)] = Tuple.Create(DataType.Integer, "int32"),
        [typeof(sbyte)] = Tuple.Create(DataType.Integer, "int32"),
        [typeof(short)] = Tuple.Create(DataType.Integer, "int32"),
        [typeof(ushort)] = Tuple.Create(DataType.Integer, "int32"),
        [typeof(int)] = Tuple.Create(DataType.Integer, "int32"),
        [typeof(uint)] = Tuple.Create(DataType.Integer, "int32"),
        [typeof(long)] = Tuple.Create(DataType.Integer, "int64"),
        [typeof(ulong)] = Tuple.Create(DataType.Integer, "int64"),
        [typeof(float)] = Tuple.Create(DataType.Number, "float"),
        [typeof(double)] = Tuple.Create(DataType.Number, "double"),
        [typeof(decimal)] = Tuple.Create(DataType.Number, "double"),
        [typeof(byte[])] = Tuple.Create(DataType.String, "byte"),
        [typeof(string)] = Tuple.Create(DataType.String, (string)null),
        [typeof(char)] = Tuple.Create(DataType.String, (string)null),
        [typeof(DateTime)] = Tuple.Create(DataType.String, "date-time"),
        [typeof(DateTimeOffset)] = Tuple.Create(DataType.String, "date-time"),
        [typeof(Guid)] = Tuple.Create(DataType.String, "uuid"),
        [typeof(Uri)] = Tuple.Create(DataType.String, "uri"),
        [typeof(TimeSpan)] = Tuple.Create(DataType.String, "date-span"),
        [typeof(DateOnly)] = Tuple.Create(DataType.String, "date"),
        [typeof(TimeOnly)] = Tuple.Create(DataType.String, "time"),
        [typeof(Int128)] = Tuple.Create(DataType.Integer, "int128"),
        [typeof(UInt128)] = Tuple.Create(DataType.Integer, "int128"),
    };
}
