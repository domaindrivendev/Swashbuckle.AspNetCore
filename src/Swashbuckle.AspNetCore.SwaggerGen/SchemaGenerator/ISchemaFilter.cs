using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.SwaggerGen;

public interface ISchemaFilter
{
    void Apply(OpenApiSchema schema, SchemaFilterContext context);
}
