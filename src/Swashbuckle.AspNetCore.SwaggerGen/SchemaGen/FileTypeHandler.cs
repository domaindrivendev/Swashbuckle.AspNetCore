using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    internal class FileTypeHandler : SchemaGeneratorHandler
    {
        public FileTypeHandler(SchemaGeneratorOptions schemaGeneratorOptions, ISchemaGenerator schemaGenerator)
            : base(schemaGeneratorOptions, schemaGenerator)
        { }

        protected override bool CanGenerateSchema(JsonContract jsonContract, out bool shouldBeRefernced)
        {
            if (typeof(IFormFile).IsAssignableFrom(jsonContract.UnderlyingType) || typeof(FileResult).IsAssignableFrom(jsonContract.UnderlyingType))
            {
                shouldBeRefernced = false;
                return true;
            }

            shouldBeRefernced = false; return false;
        }

        protected override OpenApiSchema GenerateDefinitionSchema(JsonContract jsonContract, SchemaRepository schemaRepository)
        {
            return new OpenApiSchema { Type = "string", Format = "binary" };
        }
    }
}