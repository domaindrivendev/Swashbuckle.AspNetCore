using System;
using System.Collections.Generic;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Swashbuckle.AspNetCore.Newtonsoft
{
    public class NewtonsoftPrimitiveHandler : SchemaGeneratorHandler
    {
        private readonly IContractResolver _contractResolver;

        public NewtonsoftPrimitiveHandler(IContractResolver contractResolver)
        {
            _contractResolver = contractResolver;
        }

        public override bool CanCreateSchemaFor(Type type, out bool shouldBeReferenced)
        {
            if (_contractResolver.ResolveContract(type) is JsonPrimitiveContract)
            {
                shouldBeReferenced = false;
                return true;
            }

            shouldBeReferenced = false; return false;
        }

        public override OpenApiSchema CreateSchema(Type type, SchemaRepository schemaRepository)
        {
            if (!(_contractResolver.ResolveContract(type) is JsonPrimitiveContract jsonPrimitiveContract))
               throw new InvalidOperationException($"Type {type} does not resolve to a JsonPrimitiveContract");

            return PrimitiveTypeMap.ContainsKey(type)
                ? PrimitiveTypeMap[type]()
                : new OpenApiSchema { Type = "string" };
        }

        private static readonly Dictionary<Type, Func<OpenApiSchema>> PrimitiveTypeMap = new Dictionary<Type, Func<OpenApiSchema>>
        {
            [ typeof(bool) ] = () => new OpenApiSchema { Type = "boolean" },
            [ typeof(byte) ] = () => new OpenApiSchema { Type = "integer", Format = "int32" },
            [ typeof(sbyte) ] = () => new OpenApiSchema { Type = "integer", Format = "int32" },
            [ typeof(short) ] = () => new OpenApiSchema { Type = "integer", Format = "int32" },
            [ typeof(ushort) ] = () => new OpenApiSchema { Type = "integer", Format = "int32" },
            [ typeof(int) ] = () => new OpenApiSchema { Type = "integer", Format = "int32" },
            [ typeof(uint) ] = () => new OpenApiSchema { Type = "integer", Format = "int32" },
            [ typeof(long) ] = () => new OpenApiSchema { Type = "integer", Format = "int64" },
            [ typeof(ulong) ] =  () => new OpenApiSchema { Type = "integer", Format = "int64" },
            [ typeof(float) ] = () => new OpenApiSchema { Type = "number", Format = "float" },
            [ typeof(double) ] = () => new OpenApiSchema { Type = "number", Format = "double" },
            [ typeof(decimal) ] = () => new OpenApiSchema { Type = "number", Format = "double" },
            [ typeof(byte[]) ] = () => new OpenApiSchema { Type = "string", Format = "byte" },
            [ typeof(string) ] = () => new OpenApiSchema { Type = "string" },
            [ typeof(char) ] = () => new OpenApiSchema { Type = "string"  },
            [ typeof(DateTime) ] = () => new OpenApiSchema { Type = "string", Format = "date-time" },
            [ typeof(DateTimeOffset) ] = () => new OpenApiSchema { Type = "string", Format = "date-time" },
            [ typeof(Guid) ] = () => new OpenApiSchema { Type = "string", Format = "uuid" },
            [ typeof(Uri) ] = () => new OpenApiSchema { Type = "string", Format = "uri" },
            [ typeof(TimeSpan) ] = () => new OpenApiSchema { Type = "string", Format = "date-span" }
        };
    }
}