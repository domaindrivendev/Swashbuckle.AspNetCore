using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public interface ISchemaGenerator
    {
        OpenApiSchema GenerateSchema(ModelMetadata modelMetadata, SchemaRepository schemaRepository);
    }
}
