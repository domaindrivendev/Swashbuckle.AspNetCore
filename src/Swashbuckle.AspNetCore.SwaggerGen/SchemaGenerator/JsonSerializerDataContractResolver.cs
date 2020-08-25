using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public class JsonSerializerDataContractResolver : ISerializerDataContractResolver
    {
        private readonly JsonSerializerOptions _serializerOptions;

        public JsonSerializerDataContractResolver(JsonSerializerOptions serializerOptions)
        {
            _serializerOptions = serializerOptions;
        }

        public DataContract GetDataContractForType(Type type)
        {
            if (type.IsOneOf(typeof(JsonDocument), typeof(JsonElement)))
            {
                return DataContract.ForDynamic(underlyingType: type);
            }

            if (PrimitiveTypesAndFormats.ContainsKey(type))
            {
                var primitiveTypeAndFormat = PrimitiveTypesAndFormats[type];

                return DataContract.ForPrimitive(
                    underlyingType: type,
                    dataType: primitiveTypeAndFormat.Item1,
                    dataFormat: primitiveTypeAndFormat.Item2);
            }

            if (type.IsEnum)
            {
                var enumValues = GetSerializedEnumValuesFor(type);

                var primitiveTypeAndFormat = (enumValues.Any(value => value is string))
                    ? PrimitiveTypesAndFormats[typeof(string)]
                    : PrimitiveTypesAndFormats[type.GetEnumUnderlyingType()];

                return DataContract.ForPrimitive(
                    underlyingType: type,
                    dataType: primitiveTypeAndFormat.Item1,
                    dataFormat: primitiveTypeAndFormat.Item2,
                    enumValues: enumValues);
            }

            if (IsSupportedDictionary(type, out Type keyType, out Type valueType))
            {
                return DataContract.ForDictionary(
                    underlyingType: type,
                    valueType: valueType);
            }

            if (IsSupportedCollection(type, out Type itemType))
            {
                return DataContract.ForArray(
                    underlyingType: type,
                    itemType: itemType);
            }

            return DataContract.ForObject(
                underlyingType: type,
                properties: GetDataPropertiesFor(type, out Type extensionDataType),
                extensionDataType: extensionDataType);
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

#if NETCOREAPP3_0
            if (type.IsConstructedFrom(typeof(IAsyncEnumerable<>), out constructedType))
            {
                itemType = constructedType.GenericTypeArguments[0];
                return true;
            }
#endif

            if (typeof(IEnumerable).IsAssignableFrom(type))
            {
                itemType = typeof(object);
                return true;
            }

            itemType = null;
            return false;
        }

        private IEnumerable<DataProperty> GetDataPropertiesFor(Type objectType, out Type extensionDataType)
        {
            extensionDataType = null;

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
                if (propertyInfo.HasAttribute<JsonExtensionDataAttribute>()
                    && propertyInfo.PropertyType.IsConstructedFrom(typeof(IDictionary<,>), out Type constructedDictionary))
                {
                    extensionDataType = constructedDictionary.GenericTypeArguments[1];
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
