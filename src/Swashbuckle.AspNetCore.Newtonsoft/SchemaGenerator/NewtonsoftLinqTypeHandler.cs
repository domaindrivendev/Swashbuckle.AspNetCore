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
            if (LinqTypeMap.ContainsKey(type))
            {
                shouldBeReferenced = false;
                return true;
            }

            shouldBeReferenced = false; return false;
        }

        public override OpenApiSchema CreateSchema(Type type, SchemaRepository schemaRepository)
        {
            return LinqTypeMap[type]();
        }

        private static readonly Dictionary<Type, Func<OpenApiSchema>> LinqTypeMap = new Dictionary<Type, Func<OpenApiSchema>>
        {
            [ typeof(JToken) ] = () => new OpenApiSchema(),
            [ typeof(JObject) ] = () => new OpenApiSchema { Type = "object" },
            [ typeof(JArray) ] = () => new OpenApiSchema { Type = "array", Items = new OpenApiSchema() }
        };
    }
}