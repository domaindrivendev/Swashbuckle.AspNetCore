using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public class FileSchemaGenerator : ChainableSchemaGenerator
    {
        public FileSchemaGenerator(SchemaGeneratorOptions options, ISchemaGenerator rootGenerator, IContractResolver contractResolver)
            : base(options, rootGenerator, contractResolver)
        { }

        protected override bool CanGenerateSchemaFor(Type type)
        {
            return typeof(IFormFile).IsAssignableFrom(type) || typeof(FileResult).IsAssignableFrom(type);
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