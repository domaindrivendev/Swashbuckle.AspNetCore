using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    internal class FileTypeHandler : ApiModelHandler
    {
        public FileTypeHandler(SchemaGeneratorOptions options, ISchemaGenerator generator)
            : base(options, generator)
        { }

        protected override bool CanGenerateSchema(ApiModel apiModel, out bool shouldBeReferenced)
        {
            if (typeof(IFormFile).IsAssignableFrom(apiModel.Type) || typeof(FileResult).IsAssignableFrom(apiModel.Type))
            {
                shouldBeReferenced = false;
                return true;
            }

            shouldBeReferenced = false;
            return false;
        }

        protected override OpenApiSchema GenerateDefinitionSchema(ApiModel apiModel, SchemaRepository schemaRepository)
        {
            return new OpenApiSchema { Type = "string", Format = "binary" };
        }
    }
}