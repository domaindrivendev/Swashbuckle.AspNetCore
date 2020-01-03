using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public class JsonEnumHandler : SchemaGeneratorHandler
    {
        private readonly JsonSerializerOptions _serializerOptions;

        public JsonEnumHandler(JsonSerializerOptions serializerOptions)
        {
            _serializerOptions = serializerOptions;
        }

        public override bool CanCreateSchemaFor(Type type, out bool shouldBeReferenced)
        {
            if (type.IsEnum)
            {
                shouldBeReferenced = true;
                return true;
            }

            shouldBeReferenced = false; return false;
        }

        public override OpenApiSchema CreateDefinitionSchema(Type type, SchemaRepository schemaRepository)
        {
            //Test to determine if the serializer will treat as string or not
            var describeAsString = JsonSerializer.Serialize(type.GetEnumValues().GetValue(0), _serializerOptions).StartsWith("\"");

            var schema = describeAsString
                ? EnumTypeMap[typeof(string)]()
                : EnumTypeMap[type.GetEnumUnderlyingType()]();

            if (describeAsString)
            {
                schema.Enum = type.GetEnumValues()
                    .Cast<object>()
                    .Select(value =>
                    {
                        var stringValue = JsonSerializer.Serialize(value, _serializerOptions).Replace("\"", string.Empty);
                        return OpenApiAnyFactory.CreateFor(schema, stringValue);
                    })
                    .ToList();
            }
            else
            {
                schema.Enum = type.GetEnumValues()
                    .Cast<object>()
                    .Select(value => OpenApiAnyFactory.CreateFor(schema, value))
                    .ToList();
            }

            return schema;
        }

        private static readonly Dictionary<Type, Func<OpenApiSchema>> EnumTypeMap = new Dictionary<Type, Func<OpenApiSchema>>
        {
            [ typeof(byte) ] = () => new OpenApiSchema { Type = "integer", Format = "int32" },
            [ typeof(sbyte) ] = () => new OpenApiSchema { Type = "integer", Format = "int32" },
            [ typeof(short) ] = () => new OpenApiSchema { Type = "integer", Format = "int32" },
            [ typeof(ushort) ] = () => new OpenApiSchema { Type = "integer", Format = "int32" },
            [ typeof(int) ] = () => new OpenApiSchema { Type = "integer", Format = "int32" },
            [ typeof(uint) ] = () => new OpenApiSchema { Type = "integer", Format = "int32" },
            [ typeof(long) ] = () => new OpenApiSchema { Type = "integer", Format = "int64" },
            [ typeof(ulong) ] = () => new OpenApiSchema { Type = "integer", Format = "int64" },
            [ typeof(string) ] = () => new OpenApiSchema { Type = "string" }
        };
    }
}