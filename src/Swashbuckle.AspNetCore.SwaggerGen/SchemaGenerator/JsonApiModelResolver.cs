using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public class JsonApiModelResolver : IApiModelResolver
    {
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public JsonApiModelResolver(JsonSerializerOptions jsonSerializerOptions)
        {
            _jsonSerializerOptions = jsonSerializerOptions;
        }

        public ApiModel ResolveApiModelFor(Type type)
        {
            // If it's nullable, use the inner value type
            var underlyingType = type.IsNullable(out Type innerType)
                ? innerType
                : type;

            if (underlyingType.IsValueType || underlyingType == typeof(string) || underlyingType == typeof(DateTime) || underlyingType == typeof(byte[]))
                return CreateApiPrimitive(underlyingType);

            if (underlyingType.IsDictionary(out Type keyType, out Type valueType))
                return CreateApiDictionary(underlyingType, keyType, valueType);

            if (underlyingType.IsEnumerable(out Type itemType))
                return CreateApiArray(underlyingType, itemType);

            return CreateApiObject(underlyingType);
        }

        private ApiModel CreateApiPrimitive(Type type)
        {
            if (!type.IsEnum)
                return new ApiPrimitive(type);

            // Serialize first enum value to determine if it's serialized to string or not
            var isStringEnum = JsonSerializer.Serialize(type.GetEnumValues().GetValue(0), _jsonSerializerOptions).StartsWith("\"");

            var apiEnumValues = (isStringEnum)
                ? type.GetEnumValues().Cast<object>().Select(enumValue => JsonSerializer.Serialize(enumValue, _jsonSerializerOptions).Replace("\"", string.Empty))
                : type.GetEnumValues().Cast<object>();

            return new ApiPrimitive(
                type: type,
                isStringEnum: isStringEnum,
                apiEnumValues: apiEnumValues);
        }

        private ApiModel CreateApiDictionary(Type type, Type keyType, Type valueType)
        {
            return new ApiDictionary(
                type: type,
                keyType: keyType,
                valueType: valueType);
        }

        private ApiModel CreateApiArray(Type type, Type itemType)
        {
            return new ApiArray(
                type: type,
                itemType: itemType);
        }

        private ApiModel CreateApiObject(Type type)
        {
            var serializableProperties = new List<PropertyInfo>();
            Type extensionDataValueType = null;

            foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (property.HasAttribute<JsonExtensionDataAttribute>() && property.PropertyType.IsDictionary(out Type keyType, out Type valueType))
                {
                    extensionDataValueType = valueType;
                    continue;
                }

                if (property.HasAttribute<JsonIgnoreAttribute>()) continue;

                if (property.GetIndexParameters().Any()) continue;

                if (!property.IsPubliclyReadable() && !property.IsPubliclyWritable()) continue;

                if (!property.IsPubliclyWritable() && _jsonSerializerOptions.IgnoreReadOnlyProperties) continue;

                serializableProperties.Add(property);
            }

            var apiProperties = serializableProperties
                .Select(property =>
                {
                    var nameAttribute = property.GetCustomAttribute<JsonPropertyNameAttribute>();
                    var apiName = (nameAttribute != null)
                        ? nameAttribute.Name
                        : _jsonSerializerOptions.PropertyNamingPolicy?.ConvertName(property.Name) ?? property.Name;

                    return new ApiProperty(
                        type: property.PropertyType,
                        apiName: apiName,
                        apiRequired: false,
                        apiNullable: property.PropertyType.IsReferenceOrNullableType(),
                        apiReadOnly: (property.IsPubliclyReadable() && !property.IsPubliclyWritable()),
                        apiWriteOnly: (property.IsPubliclyWritable() && !property.IsPubliclyReadable()),
                        memberInfo: property);
                });

            return new ApiObject(
                type: type,
                apiProperties: apiProperties,
                additionalPropertiesType: extensionDataValueType);
        }
    }
}
