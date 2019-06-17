using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    internal class FallbackHandler : SchemaGeneratorHandler
    {
        internal FallbackHandler(SchemaGeneratorOptions schemaGeneratorOptions, ISchemaGenerator schemaGenerator, JsonSerializerSettings jsonSerializerSettings)
            : base(schemaGeneratorOptions, schemaGenerator, jsonSerializerSettings)
        { }

        protected override bool CanGenerateSchemaFor(ModelMetadata modelMetadata, JsonContract jsonContract)
        {
            return true;
        }

        protected override OpenApiSchema GenerateSchemaFor(ModelMetadata modelMetadata, SchemaRepository schemaRepository, JsonContract jsonContract)
        {
            return new OpenApiSchema { Type = "string" };
        }
    }
}