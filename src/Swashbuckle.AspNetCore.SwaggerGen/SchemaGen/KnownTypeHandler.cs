using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    internal class KnownTypeHandler : SchemaGeneratorHandler
    {
        public KnownTypeHandler(SchemaGeneratorOptions schemaGeneratorOptions, ISchemaGenerator schemaGenerator, JsonSerializerSettings jsonSerializerSettings)
            : base(schemaGeneratorOptions, schemaGenerator, jsonSerializerSettings)
        { }

        protected override bool CanGenerateSchemaFor(ModelMetadata modelMetadata, JsonContract jsonContract)
        {
            var modelType = modelMetadata.ModelType;

            return SchemaGeneratorOptions.CustomTypeMappings.ContainsKey(modelType)
                || KnownTypeMappings.ContainsKey(modelType)
                || typeof(IFormFile).IsAssignableFrom(modelType)
                || typeof(FileResult).IsAssignableFrom(modelType);
        }

        protected override OpenApiSchema GenerateSchemaFor(ModelMetadata modelMetadata, SchemaRepository schemaRepository, JsonContract jsonContract)
        {
            var modelType = modelMetadata.ModelType;

            if (SchemaGeneratorOptions.CustomTypeMappings.ContainsKey(modelType))
                return SchemaGeneratorOptions.CustomTypeMappings[modelType]();

            if (KnownTypeMappings.ContainsKey(modelType))
                return KnownTypeMappings[modelType]();

            return new OpenApiSchema { Type = "string", Format = "binary" };
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
