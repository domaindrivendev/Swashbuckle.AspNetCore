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
            if (type.IsOneOf(typeof(object), typeof(JsonDocument), typeof(JsonElement)))
            {
                return DataContract.ForDynamic(
                    underlyingType: type,
                    jsonConverter: JsonConverterFunc);
            }

            if (PrimitiveTypesAndFormats.ContainsKey(type))
            {
                var primitiveTypeAndFormat = PrimitiveTypesAndFormats[type];

                return DataContract.ForPrimitive(
                    underlyingType: type,
                    dataType: primitiveTypeAndFormat.Item1,
                    dataFormat: primitiveTypeAndFormat.Item2,
                    jsonConverter: JsonConverterFunc);
            }

            if (type.IsEnum)
            {
                var enumValues = type.GetEnumValues();

                //Test to determine if the serializer will treat as string
                var serializeAsString = (enumValues.Length > 0)
                    && JsonConverterFunc(enumValues.GetValue(0)).StartsWith("\"");

                var primitiveTypeAndFormat = serializeAsString
                    ? PrimitiveTypesAndFormats[typeof(string)]
                    : PrimitiveTypesAndFormats[type.GetEnumUnderlyingType()];

                return DataContract.ForPrimitive(
                    underlyingType: type,
                    dataType: primitiveTypeAndFormat.Item1,
                    dataFormat: primitiveTypeAndFormat.Item2,
                    jsonConverter: JsonConverterFunc);
            }

            if (IsSupportedDictionary(type, out Type keyType, out Type valueType))
            {
                return DataContract.ForDictionary(
                    underlyingType: type,
                    valueType: valueType,
                    keys: null, // STJ doesn't currently support dictionaries with enum key types
                    jsonConverter: JsonConverterFunc);
            }

            if (IsSupportedCollection(type, out Type itemType))
            {
                return DataContract.ForArray(
                    underlyingType: type,
                    itemType: itemType,
                    jsonConverter: JsonConverterFunc);
            }

            return DataContract.ForObject(
                underlyingType: type,
                properties: GetDataPropertiesFor(type, out Type extensionDataType),
                extensionDataType: extensionDataType,
                jsonConverter: JsonConverterFunc);
        }

        private string JsonConverterFunc(object value)
        {
            return JsonSerializer.Serialize(value, _serializerOptions);
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

#if (!NETSTANDARD2_0)
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

            const BindingFlags PublicBindingAttr = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            var publicProperties = objectType.IsInterface
                ? new[] { objectType }.Concat(objectType.GetInterfaces()).SelectMany(i => i.GetProperties(PublicBindingAttr))
                : objectType.GetProperties(PublicBindingAttr);

            var applicableProperties = publicProperties
                .Where(property =>
                {
                    // .Net 5 introduces JsonIgnoreAttribute.Condition which should be honored
                    bool isIgnoredViaNet5Attribute = true;

#if NET5_0
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
#endif

                    return
                        (property.IsPubliclyReadable() || property.IsPubliclyWritable()) &&
                        !(property.GetIndexParameters().Any()) &&
                        !(property.HasAttribute<JsonIgnoreAttribute>() && isIgnoredViaNet5Attribute) &&
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
