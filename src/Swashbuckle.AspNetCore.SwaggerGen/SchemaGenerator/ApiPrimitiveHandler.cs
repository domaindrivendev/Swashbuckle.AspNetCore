using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    internal class ApiPrimitiveHandler : ApiModelHandler
    {
        public ApiPrimitiveHandler( SchemaGeneratorOptions options, ISchemaGenerator generator)
            : base(options, generator)
        { }

        protected override bool CanGenerateSchema(ApiModel apiModel, out bool shouldBeReferenced)
        {
            if (apiModel is ApiPrimitive apiPrimitive)
            {
                shouldBeReferenced = apiPrimitive.IsEnum;
                return true;
            }

            shouldBeReferenced = false; return false;
        }

        protected override OpenApiSchema GenerateDefinitionSchema(ApiModel apiModel, SchemaRepository schemaRepository)
        {
            var apiPrimitive = (ApiPrimitive)apiModel;

            if (apiPrimitive.IsEnum)
                return GenerateEnumSchema(apiPrimitive);

            return PrimitiveTypeMap.ContainsKey(apiPrimitive.Type)
                ? PrimitiveTypeMap[apiPrimitive.Type]()
                : PrimitiveTypeMap[typeof(string)](); // if no mapping exists, default to string
        }

        private OpenApiSchema GenerateEnumSchema(ApiPrimitive apiPrimitive)
        {
            var schema = apiPrimitive.IsStringEnum
                ? PrimitiveTypeMap[typeof(string)]()
                : PrimitiveTypeMap[apiPrimitive.Type.GetEnumUnderlyingType()]();

            schema.Enum = apiPrimitive.ApiEnumValues
                .Select(value =>
                {
                    return OpenApiAnyFactory.TryCreateFor(schema, value, out IOpenApiAny openApiAny) ? openApiAny : null;
                })
                .ToList();

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