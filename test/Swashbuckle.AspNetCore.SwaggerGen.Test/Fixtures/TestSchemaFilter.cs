using Microsoft.OpenApi;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test;

public class TestSchemaFilter : ISchemaFilter
{
    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema is OpenApiSchema openApiSchema)
        {
            openApiSchema.Extensions ??= new Dictionary<string, IOpenApiExtension>();
            openApiSchema.Extensions.Add("X-foo", new JsonNodeExtension("bar"));
            openApiSchema.Extensions.Add("X-docName", new JsonNodeExtension(context.DocumentName));
        }
    }
}
