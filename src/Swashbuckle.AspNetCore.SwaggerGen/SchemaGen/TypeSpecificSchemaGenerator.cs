using System;
using System.Collections.Generic;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public class TypeSpecificSchemaGenerator : ChainableSchemaGenerator
    {
        public TypeSpecificSchemaGenerator(
            IContractResolver contractResolver,
            ISchemaGenerator rootGenerator,
            SchemaGeneratorOptions options)
            : base(contractResolver, rootGenerator, options)
        { }

        protected override bool CanGenerateSchemaFor(Type type)
        {
            return Options.CustomTypeMappings.ContainsKey(type) || KnownTypeMappings.ContainsKey(type);
        }

        protected override OpenApiSchema GenerateSchemaFor(Type type, SchemaRepository schemaRepository)
        {
            return Options.CustomTypeMappings.ContainsKey(type)
                ? Options.CustomTypeMappings[type]()
                : KnownTypeMappings[type]();
        }

        private static Dictionary<Type, Func<OpenApiSchema>> KnownTypeMappings = new Dictionary<Type, Func<OpenApiSchema>>
        {
            [ typeof(object) ] = () => new OpenApiSchema { Type = "object" },
            [ typeof(JToken) ] = () => new OpenApiSchema { Type = "object" },
            [ typeof(JObject) ] = () => new OpenApiSchema { Type = "object" },
            [ typeof(JArray) ] = () => new OpenApiSchema { Type = "array", Items = new OpenApiSchema { Type = "object" } }
        };
    }
}
