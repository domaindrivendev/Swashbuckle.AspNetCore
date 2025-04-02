using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models.Interfaces;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test;

public class TestSchemaFilter : ISchemaFilter
{
    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        schema.Extensions.Add("X-foo", new OpenApiAny("bar"));
        schema.Extensions.Add("X-docName", new OpenApiAny(context.DocumentName));
    }
}
