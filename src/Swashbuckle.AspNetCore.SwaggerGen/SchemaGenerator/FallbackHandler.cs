using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    internal class FallbackHandler : ApiModelHandler
    {
        public FallbackHandler(SchemaGeneratorOptions options, ISchemaGenerator schemaGenerator)
            : base(options, schemaGenerator)
        { }

        protected override bool CanGenerateSchema(ApiModel apiModel, out bool shouldBeReferenced)
        {
            shouldBeReferenced = false;
            return true;
        }

        protected override OpenApiSchema GenerateDefinitionSchema(ApiModel apiModel, SchemaRepository schemaRepository)
        {
            return new OpenApiSchema { Type = "string" };
        }
    }
}