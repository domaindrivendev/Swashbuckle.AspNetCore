using Microsoft.OpenApi.Models.Interfaces;

namespace Swashbuckle.AspNetCore.SwaggerGen;

public interface ISchemaFilter
{
    void Apply(IOpenApiSchema schema, SchemaFilterContext context);
}
