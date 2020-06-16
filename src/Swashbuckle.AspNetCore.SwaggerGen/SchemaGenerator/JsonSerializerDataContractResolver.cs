using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public class JsonSerializerDataContractResolver : IDataContractResolver
    {
        private readonly JsonSerializerOptions _serializerOptions;

        public JsonSerializerDataContractResolver(JsonSerializerOptions serializerOptions)
        {
            _serializerOptions = serializerOptions;
        }

        public DataContract GetDataContractForType(Type type)
        {
            var underlyingType = type.IsNullable(out Type innerType) ? innerType : type;

            if (PrimitiveTypesAndFormats.ContainsKey(underlyingType))
            {
                var primitiveTypeAndFormat = PrimitiveTypesAndFormats[underlyingType];

                return new DataContract(
                    dataType: primitiveTypeAndFormat.Item1,
                    format: primitiveTypeAndFormat.Item2,
                    underlyingType: underlyingType);
            }

            if (underlyingType.IsEnum)
            {
                var enumValues = GetSerializedEnumValuesFor(underlyingType);

                var primitiveTypeAndFormat = (enumValues.Any(value => value is string))
                    ? PrimitiveTypesAndFormats[typeof(string)]
                    : PrimitiveTypesAndFormats[underlyingType.GetEnumUnderlyingType()];

                return new DataContract(
                    dataType: primitiveTypeAndFormat.Item1,
                    format: primitiveTypeAndFormat.Item2,
                    underlyingType: underlyingType,
                    enumValues: enumValues);
            }

            if (underlyingType.IsDictionary(out Type keyType, out Type valueType))
            {
                if (keyType.IsEnum)
                    throw new NotSupportedException(
                        $"Schema cannot be generated for type {underlyingType} as it's not supported by the System.Text.Json serializer");

                return new DataContract(
                    dataType: DataType.Object,
                    underlyingType: underlyingType,
                    additionalPropertiesType: valueType);
            }

            if (underlyingType.IsEnumerable(out Type itemType))
            {
                return new DataContract(
                    dataType: DataType.Array,
                    underlyingType: underlyingType,
                    arrayItemType: itemType);
            }

            if (underlyingType.IsOneOf(typeof(JsonDocument), typeof(JsonElement)))
            {
                return new DataContract(
                    dataType: DataType.Unknown,
                    underlyingType: underlyingType);
            }

            return new DataContract(
                dataType: DataType.Object,
                underlyingType: underlyingType,
                properties: GetDataPropertiesFor(underlyingType, out Type extensionDataValueType),
                additionalPropertiesType: extensionDataValueType);
        }

        private IEnumerable<object> GetSerializedEnumValuesFor(Type enumType)
        {
            var underlyingValues = enumType.GetEnumValues().Cast<object>();

            //Test to determine if the serializer will treat as string or not
            var serializeAsString = underlyingValues.Any()
                && JsonSerializer.Serialize(underlyingValues.First(), _serializerOptions).StartsWith("\"");

            return serializeAsString
                ? underlyingValues.Select(value => JsonSerializer.Serialize(value, _serializerOptions).Replace("\"", string.Empty))
                : underlyingValues;
        }

        private IEnumerable<DataProperty> GetDataPropertiesFor(Type objectType, out Type extensionDataValueType)
        {
            extensionDataValueType = null;

            if (objectType == typeof(object))
            {
                return null;
            }

            var applicableProperties = objectType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(property =>
                {
                    return
                        (property.IsPubliclyReadable() || property.IsPubliclyWritable()) &&
                        !(property.GetIndexParameters().Any()) &&
                        !(property.HasAttribute<JsonIgnoreAttribute>()) &&
                        !(_serializerOptions.IgnoreReadOnlyProperties && !property.IsPubliclyWritable());
                });

            var dataProperties = new List<DataProperty>();

            foreach (var propertyInfo in applicableProperties)
            {
                if (propertyInfo.HasAttribute<JsonExtensionDataAttribute>() && propertyInfo.PropertyType.IsDictionary(out Type _, out Type valueType))
                {
                    extensionDataValueType = valueType;
                    continue;
                }

                var name = propertyInfo.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name
                    ?? _serializerOptions.PropertyNamingPolicy?.ConvertName(propertyInfo.Name) ?? propertyInfo.Name;

                dataProperties.Add(
                    new DataProperty(
                        name: name,
                        isRequired: false,
                        isNullable: propertyInfo.PropertyType.IsReferenceOrNullableType(),
                        isReadOnly: propertyInfo.IsPubliclyReadable() && !propertyInfo.IsPubliclyWritable(),
                        isWriteOnly: propertyInfo.IsPubliclyWritable() && !propertyInfo.IsPubliclyReadable(),
                        memberType: propertyInfo.PropertyType,
                        memberInfo: propertyInfo));
            }

            return dataProperties;
        }

        private static readonly Dictionary<Type, Tuple<DataType, string>> PrimitiveTypesAndFormats = new Dictionary<Type, Tuple<DataType, string>>
        {
            [ typeof(bool) ] = Tuple.Create(DataType.Boolean, (string)null),
            [ typeof(byte) ] = Tuple.Create(DataType.Integer, "int32"),
            [ typeof(sbyte) ] = Tuple.Create(DataType.Integer, "int32"),
            [ typeof(short) ] = Tuple.Create(DataType.Integer, "int32"),
            [ typeof(ushort) ] = Tuple.Create(DataType.Integer, "int32"),
            [ typeof(int) ] = Tuple.Create(DataType.Integer, "int32"),
            [ typeof(uint) ] = Tuple.Create(DataType.Integer, "int32"),
            [ typeof(long) ] = Tuple.Create(DataType.Integer, "int64"),
            [ typeof(ulong) ] = Tuple.Create(DataType.Integer, "int64"),
            [ typeof(float) ] = Tuple.Create(DataType.Number, "float"),
            [ typeof(double) ] = Tuple.Create(DataType.Number, "double"),
            [ typeof(decimal) ] = Tuple.Create(DataType.Number, "double"),
            [ typeof(byte[]) ] = Tuple.Create(DataType.String, "byte"),
            [ typeof(string) ] = Tuple.Create(DataType.String, (string)null),
            [ typeof(char) ] = Tuple.Create(DataType.String, (string)null),
            [ typeof(DateTime) ] = Tuple.Create(DataType.String, "date-time"),
            [ typeof(DateTimeOffset) ] = Tuple.Create(DataType.String, "date-time"),
            [ typeof(Guid) ] = Tuple.Create(DataType.String, "uuid"),
            [ typeof(Uri) ] = Tuple.Create(DataType.String, "uri")
        };
    }
}
