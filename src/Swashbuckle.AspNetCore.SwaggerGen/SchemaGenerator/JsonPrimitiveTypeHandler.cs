using System;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public class JsonPrimitiveTypeHandler : SchemaGeneratorHandler
    {
        private readonly JsonSerializerOptions _serializerOptions;

        public JsonPrimitiveTypeHandler(JsonSerializerOptions serializerOptions)
        {
            _serializerOptions = serializerOptions;
        }

        public override bool CanCreateSchemaFor(Type type, out bool shouldBeReferenced)
        {
            if (PrimitiveTypeMap.ContainsKey(type) || (type.IsNullable(out Type innerType) && PrimitiveTypeMap.ContainsKey(innerType)))
            {
                shouldBeReferenced = false;
                return true;
            }

            shouldBeReferenced = false; return false;
        }

        public override OpenApiSchema CreateDefinitionSchema(Type type, SchemaRepository schemaRepository)
        {
            var isNullable = type.IsNullable(out Type innerType);

            var schema = isNullable
                ? PrimitiveTypeMap[innerType]()
                : PrimitiveTypeMap[type]();

            if (_serializerOptions.IgnoreNullValues)
            {
                schema.Nullable = false;
            }
            else
            {
                schema.Nullable = (!type.IsValueType || isNullable);
            }

            return schema;
        }

        private static readonly Dictionary<Type, Func<OpenApiSchema>> PrimitiveTypeMap = new Dictionary<Type, Func<OpenApiSchema>>
        {
            { typeof(bool), () => new OpenApiSchema { Type = "boolean" } },
            { typeof(byte), () => new OpenApiSchema { Type = "integer", Format = "int32" } },
            { typeof(sbyte), () => new OpenApiSchema { Type = "integer", Format = "int32" } },
            { typeof(short), () => new OpenApiSchema { Type = "integer", Format = "int32" } },
            { typeof(ushort), () => new OpenApiSchema { Type = "integer", Format = "int32" } },
            { typeof(int), () => new OpenApiSchema { Type = "integer", Format = "int32" } },
            { typeof(uint), () => new OpenApiSchema { Type = "integer", Format = "int32" } },
            { typeof(long), () => new OpenApiSchema { Type = "integer", Format = "int64" } },
            { typeof(ulong), () => new OpenApiSchema { Type = "integer", Format = "int64" } },
            { typeof(float), () => new OpenApiSchema { Type = "number", Format = "float" } },
            { typeof(double), () => new OpenApiSchema { Type = "number", Format = "double" } },
            { typeof(decimal), () => new OpenApiSchema { Type = "number", Format = "double" } },
            { typeof(byte[]), () => new OpenApiSchema { Type = "string", Format = "byte" } },
            { typeof(string), () => new OpenApiSchema { Type = "string" } },
            { typeof(char), () => new OpenApiSchema { Type = "string" } },
            { typeof(DateTime), () => new OpenApiSchema { Type = "string", Format = "date-time" } },
            { typeof(DateTimeOffset), () => new OpenApiSchema { Type = "string", Format = "date-time" } },
            { typeof(Guid), () => new OpenApiSchema { Type = "string", Format = "uuid" } },
            { typeof(Uri), () => new OpenApiSchema { Type = "string", Format = "uri" } },
            { typeof(TimeSpan), () => new OpenApiSchema { Type = "string", Format = "date-span" } },
        };
    }
}