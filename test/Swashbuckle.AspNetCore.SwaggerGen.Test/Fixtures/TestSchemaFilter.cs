using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Models.Interfaces;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test;

public class TestSchemaFilter : ISchemaFilter
{
    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema is OpenApiSchema openApiSchema)
        {
            openApiSchema.Extensions ??= [];
            openApiSchema.Extensions.Add("X-foo", new JsonNodeExtension("bar"));
            openApiSchema.Extensions.Add("X-docName", new JsonNodeExtension(context.DocumentName));
        }
    }
}
