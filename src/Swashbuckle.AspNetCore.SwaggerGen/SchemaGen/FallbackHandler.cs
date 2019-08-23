using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public class FallbackHandler : SchemaGeneratorHandler
    {
        public FallbackHandler(SchemaGeneratorOptions schemaGeneratorOptions, ISchemaGenerator schemaGenerator)
            : base(schemaGeneratorOptions, schemaGenerator)
        { }

        protected override bool CanGenerateSchema(JsonContract jsonContract, out bool shouldBeReferenced)
        {
            shouldBeReferenced = false;
            return true;
        }

        protected override OpenApiSchema GenerateDefinitionSchema(JsonContract jsonContract, SchemaRepository schemaRepository)
        {
            return new OpenApiSchema
            {
                Type = "object"
            };
        }
    }
}