using System;
using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public class FormFileSchemaGenerator : ChainableSchemaGenerator
    {
        public FormFileSchemaGenerator(SchemaGeneratorOptions options, ISchemaGenerator rootGenerator, IContractResolver contractResolver)
            : base(options, rootGenerator, contractResolver)
        { }

        protected override bool CanGenerateSchemaFor(Type type)
        {
            return typeof(IFormFile).IsAssignableFrom(type);
        }

        protected override OpenApiSchema GenerateSchemaFor(Type type, SchemaRepository schemaRepository)
        {
            return new OpenApiSchema
            {
                Type = "string",
                Format = "binary"
            };
        }
    }
}