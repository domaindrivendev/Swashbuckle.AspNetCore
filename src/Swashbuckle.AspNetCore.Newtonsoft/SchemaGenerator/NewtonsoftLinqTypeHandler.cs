using System;
using System.Collections.Generic;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Linq;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Swashbuckle.AspNetCore.Newtonsoft
{
    public class NewtonsoftLinqTypeHandler : SchemaGeneratorHandler
    {
        public override bool CanCreateSchemaFor(Type type, out bool shouldBeReferenced)
        {
            if (type.IsOneOf(typeof(JToken), typeof(JObject), typeof(JArray)))
            {
                shouldBeReferenced = false;
                return true;
            }

            shouldBeReferenced = false; return false;
        }

        public override OpenApiSchema CreateDefinitionSchema(Type type, SchemaRepository schemaRepository)
        {
            var schema = LinqTypeMap[type]();

            schema.Nullable = true;

            return schema;
        }

        private static readonly Dictionary<Type, Func<OpenApiSchema>> LinqTypeMap = new Dictionary<Type, Func<OpenApiSchema>>
        {
            { typeof(JToken), () => new OpenApiSchema { Type = "object" } },
            { typeof(JObject), () => new OpenApiSchema { Type = "object" } },
            { typeof(JArray), () => new OpenApiSchema { Type = "array", Items = new OpenApiSchema { Type = "object" } } }
        };
    }
}