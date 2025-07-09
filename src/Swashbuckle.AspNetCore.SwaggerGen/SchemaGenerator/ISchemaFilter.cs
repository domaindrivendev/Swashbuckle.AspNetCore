using Microsoft.OpenApi;

namespace Swashbuckle.AspNetCore.SwaggerGen;

public interface ISchemaFilter
{
    void Apply(IOpenApiSchema schema, SchemaFilterContext context);
}
