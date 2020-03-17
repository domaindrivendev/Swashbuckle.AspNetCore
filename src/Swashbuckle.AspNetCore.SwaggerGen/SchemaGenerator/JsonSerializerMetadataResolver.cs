using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public class JsonSerializerMetadataResolver : ISerializerMetadataResolver
    {
        private readonly JsonSerializerOptions _serializerOptions;

        public JsonSerializerMetadataResolver(JsonSerializerOptions serializerOptions)
        {
            _serializerOptions = serializerOptions;
        }

        public SerializerMetadata GetSerializerMetadataForType(Type type)
        {
            var underlyingType = type.IsNullable(out Type innerType) ? innerType : type;

            if (PrimitiveTypesAndFormats.ContainsKey(underlyingType))
            {
                var primitiveTypeAndFormat = PrimitiveTypesAndFormats[underlyingType];
                return SerializerMetadata.ForPrimitive(underlyingType, primitiveTypeAndFormat.Item1, primitiveTypeAndFormat.Item2);
            }

            if (underlyingType.IsEnum)
            {
                var enumValues = GetSerializeEnumValuesFor(underlyingType);

                var primitiveTypeAndFormat = (enumValues.Any(value => value.GetType() == typeof(string)))
                    ? PrimitiveTypesAndFormats[typeof(string)]
                    : PrimitiveTypesAndFormats[underlyingType.GetEnumUnderlyingType()];

                return SerializerMetadata.ForPrimitive(underlyingType, primitiveTypeAndFormat.Item1, primitiveTypeAndFormat.Item2, enumValues);
            }

            if (underlyingType.IsDictionary(out Type keyType, out Type valueType))
            {
                return SerializerMetadata.ForDictionary(underlyingType, keyType, valueType);
            }

            if (underlyingType.IsEnumerable(out Type itemType))
            {
                return SerializerMetadata.ForArray(underlyingType, itemType);
            }

            return SerializerMetadata.ForObject(
                underlyingType,
                GetSerializerPropertiesFor(underlyingType, out Type extensionDataValueType),
                extensionDataValueType);
        }

        private IEnumerable<object> GetSerializeEnumValuesFor(Type enumType)
        {
            var underlyingValues = enumType.GetEnumValues().Cast<object>();

            //Test to determine if the serializer will treat as string or not
            var serializeAsString = underlyingValues.Any() && JsonSerializer.Serialize(underlyingValues.First(), _serializerOptions).StartsWith("\"");

            return serializeAsString
                ? underlyingValues.Select(value => JsonSerializer.Serialize(value, _serializerOptions).Replace("\"", string.Empty))
                : underlyingValues;
        }

        private IEnumerable<SerializerPropertyMetadata> GetSerializerPropertiesFor(Type objectType, out Type extensionDataValueType)
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

            var serializerProperties = new List<SerializerPropertyMetadata>();

            foreach (var propertyInfo in applicableProperties)
            {
                if (propertyInfo.HasAttribute<JsonExtensionDataAttribute>() && propertyInfo.PropertyType.IsDictionary(out Type _, out Type valueType))
                {
                    extensionDataValueType = valueType;
                    continue;
                }

                var name = propertyInfo.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name
                    ?? _serializerOptions.PropertyNamingPolicy?.ConvertName(propertyInfo.Name) ?? propertyInfo.Name;

                serializerProperties.Add(
                    new SerializerPropertyMetadata(
                        name: name,
                        memberType: propertyInfo.PropertyType,
                        memberInfo: propertyInfo,
                        allowNull: propertyInfo.PropertyType.IsReferenceOrNullableType()));
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
            [ typeof(Uri) ] = Tuple.Create("string", "uri")
        };
    }
}
