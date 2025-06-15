using System.Collections;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Swashbuckle.AspNetCore.Annotations;

namespace Swashbuckle.AspNetCore.SwaggerGen;

public class JsonSerializerDataContractResolver(JsonSerializerOptions serializerOptions) : ISerializerDataContractResolver
{
    private readonly JsonSerializerOptions _serializerOptions = serializerOptions;

    public DataContract GetDataContractForType(Type type)
    {
        var effectiveType = Nullable.GetUnderlyingType(type) ?? type;
        if (effectiveType.IsOneOf(typeof(object), typeof(JsonDocument), typeof(JsonElement)))
        {
            return DataContract.ForDynamic(
                underlyingType: effectiveType,
                jsonConverter: (value) => JsonConverterFunc(value, effectiveType));
        }

        if (PrimitiveTypesAndFormats.TryGetValue(effectiveType, out var primitiveTypeAndFormat))
        {
            return DataContract.ForPrimitive(
                underlyingType: effectiveType,
                dataType: primitiveTypeAndFormat.Item1,
                dataFormat: primitiveTypeAndFormat.Item2,
                jsonConverter: (value) => JsonConverterFunc(value, type));
        }

        if (effectiveType.IsEnum)
        {
            var enumValues = effectiveType.GetEnumValues();

            // Test to determine if the serializer will treat as string
            var serializeAsString =
                enumValues.Length > 0 &&
                JsonConverterFunc(enumValues.GetValue(0), type).StartsWith('\"');

            var exampleType = serializeAsString ?
                typeof(string) :
                effectiveType.GetEnumUnderlyingType();

            primitiveTypeAndFormat = PrimitiveTypesAndFormats[exampleType];

            return DataContract.ForPrimitive(
                underlyingType: effectiveType,
                dataType: primitiveTypeAndFormat.Item1,
                dataFormat: primitiveTypeAndFormat.Item2,
                jsonConverter: (value) => JsonConverterFunc(value, type));
        }

        if (IsSupportedDictionary(effectiveType, out Type keyType, out Type valueType))
        {
            IEnumerable<string> keys = null;

            if (keyType.IsEnum)
            {
                // This is a special case where we know the possible key values
                var enumValuesAsJson = keyType.GetEnumValues()
                    .Cast<object>()
                    .Select(value => JsonConverterFunc(value, keyType));

                keys =
                    enumValuesAsJson.Any(json => json.StartsWith('\"'))
                    ? enumValuesAsJson.Select(json => json.Replace("\"", string.Empty))
                    : keyType.GetEnumNames();
            }

            return DataContract.ForDictionary(
                underlyingType: effectiveType,
                valueType: valueType,
                keys: keys,
                jsonConverter: (value) => JsonConverterFunc(value, effectiveType));
        }

        if (IsSupportedCollection(effectiveType, out Type itemType))
        {
            return DataContract.ForArray(
                underlyingType: effectiveType,
                itemType: itemType,
                jsonConverter: (value) => JsonConverterFunc(value, effectiveType));
        }

        return DataContract.ForObject(
            underlyingType: effectiveType,
            properties: GetDataPropertiesFor(effectiveType, out Type extensionDataType),
            extensionDataType: extensionDataType,
            jsonConverter: (value) => JsonConverterFunc(value, effectiveType));
    }

    private string JsonConverterFunc(object value, Type type)
    {
        return JsonSerializer.Serialize(value, type, _serializerOptions);
    }

    public bool IsSupportedDictionary(Type type, out Type keyType, out Type valueType)
    {
        if (type.IsConstructedFrom(typeof(IDictionary<,>), out Type constructedType)
            || type.IsConstructedFrom(typeof(IReadOnlyDictionary<,>), out constructedType))
        {
            keyType = constructedType.GenericTypeArguments[0];
            valueType = constructedType.GenericTypeArguments[1];
            return true;
        }

        if (typeof(IDictionary).IsAssignableFrom(type))
        {
            keyType = valueType = typeof(object);
            return true;
        }

        keyType = valueType = null;
        return false;
    }

    public bool IsSupportedCollection(Type type, out Type itemType)
    {
        if (type.IsConstructedFrom(typeof(IEnumerable<>), out Type constructedType))
        {
            itemType = constructedType.GenericTypeArguments[0];
            return true;
        }

        if (type.IsConstructedFrom(typeof(IAsyncEnumerable<>), out constructedType))
        {
            itemType = constructedType.GenericTypeArguments[0];
            return true;
        }

        if (type.IsArray)
        {
            itemType = type.GetElementType();
            return true;
        }

        if (typeof(IEnumerable).IsAssignableFrom(type))
        {
            itemType = typeof(object);
            return true;
        }

        itemType = null;
        return false;
    }

    private List<DataProperty> GetDataPropertiesFor(Type objectType, out Type extensionDataType)
    {
        extensionDataType = null;

        const BindingFlags PublicBindingAttr = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        var publicProperties = objectType.IsInterface
            ? new[] { objectType }.Concat(objectType.GetInterfaces()).SelectMany(i => i.GetProperties(PublicBindingAttr))
            : objectType.GetProperties(PublicBindingAttr);

        var applicableProperties = publicProperties
            .Where(property =>
            {
                // .NET 5 introduces JsonIgnoreAttribute.Condition which should be honored
                bool isIgnoredViaNet5Attribute = true;

                JsonIgnoreAttribute jsonIgnoreAttribute = property.GetCustomAttribute<JsonIgnoreAttribute>();
                if (jsonIgnoreAttribute != null)
                {
                    isIgnoredViaNet5Attribute = jsonIgnoreAttribute.Condition switch
                    {
                        JsonIgnoreCondition.Never => false,
                        JsonIgnoreCondition.Always => true,
                        JsonIgnoreCondition.WhenWritingDefault => false,
                        JsonIgnoreCondition.WhenWritingNull => false,
                        _ => true
                    };
                }

                return
                    (property.IsPubliclyReadable() || property.IsPubliclyWritable()) &&
                    !(property.GetIndexParameters().Length > 0) &&
                    !(property.HasAttribute<JsonIgnoreAttribute>() && isIgnoredViaNet5Attribute) &&
                    !property.HasAttribute<SwaggerIgnoreAttribute>() &&
                    !(_serializerOptions.IgnoreReadOnlyProperties && !property.IsPubliclyWritable());
            })
            .OrderBy(property => property.DeclaringType.GetInheritanceChain().Length);

        var dataProperties = new List<DataProperty>();

        foreach (var propertyInfo in applicableProperties)
        {
            if (propertyInfo.HasAttribute<JsonExtensionDataAttribute>()
                && propertyInfo.PropertyType.IsConstructedFrom(typeof(IDictionary<,>), out Type constructedDictionary))
            {
                extensionDataType = constructedDictionary.GenericTypeArguments[1];
                continue;
            }

            var name = propertyInfo.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name
                ?? _serializerOptions.PropertyNamingPolicy?.ConvertName(propertyInfo.Name) ?? propertyInfo.Name;

            // .NET 5 introduces support for serializing immutable types via parameterized constructors
            // See https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-immutability?pivots=dotnet-6-0
            var isDeserializedViaConstructor = false;

            var isRequired = false;

            var deserializationConstructor = propertyInfo.DeclaringType?.GetConstructors()
                .OrderBy(c =>
                {
                    if (c.GetCustomAttribute<JsonConstructorAttribute>() != null)
                    {
                        return 1;
                    }
                    else if (c.GetParameters().Length == 0)
                    {
                        return 2;
                    }

                    return 3;
                })
                .FirstOrDefault();

            isDeserializedViaConstructor = deserializationConstructor != null && deserializationConstructor.GetParameters()
                .Any(p =>
                {
                    return
                        string.Equals(p.Name, propertyInfo.Name, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase);
                });

            isRequired = propertyInfo.GetCustomAttribute<JsonRequiredAttribute>() != null;

            dataProperties.Add(
                new DataProperty(
                    name: name,
                    isRequired: isRequired,
                    isNullable: propertyInfo.PropertyType.IsReferenceOrNullableType(),
                    isReadOnly: propertyInfo.IsPubliclyReadable() && !propertyInfo.IsPubliclyWritable() && !isDeserializedViaConstructor,
                    isWriteOnly: propertyInfo.IsPubliclyWritable() && !propertyInfo.IsPubliclyReadable(),
                    memberType: propertyInfo.PropertyType,
                    memberInfo: propertyInfo));
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
        [typeof(TimeSpan)] = Tuple.Create(DataType.String, "date-span"),
        [typeof(Guid)] = Tuple.Create(DataType.String, "uuid"),
        [typeof(Uri)] = Tuple.Create(DataType.String, "uri"),
        [typeof(Version)] = Tuple.Create(DataType.String, (string)null),
        [typeof(DateOnly)] = Tuple.Create(DataType.String, "date"),
        [typeof(TimeOnly)] = Tuple.Create(DataType.String, "time"),
        [typeof(Int128)] = Tuple.Create(DataType.Integer, "int128"),
        [typeof(UInt128)] = Tuple.Create(DataType.Integer, "int128"),
    };
}
