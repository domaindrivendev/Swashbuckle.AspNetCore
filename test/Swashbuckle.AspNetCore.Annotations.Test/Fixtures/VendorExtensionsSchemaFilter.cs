using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Swashbuckle.AspNetCore.Annotations.Test;

public class VendorExtensionsSchemaFilter : ISchemaFilter
{
    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema is OpenApiSchema openApiSchema)
        {
            openApiSchema.Extensions ??= new Dictionary<string, IOpenApiExtension>();
            openApiSchema.Extensions.Add("X-property1", new JsonNodeExtension("value"));
        }
    }
}
