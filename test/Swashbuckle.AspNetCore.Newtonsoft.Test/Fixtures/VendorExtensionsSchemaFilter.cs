using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Models.Interfaces;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Swashbuckle.AspNetCore.Newtonsoft.Test;

public class VendorExtensionsSchemaFilter : ISchemaFilter
{
    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema is OpenApiSchema openApiSchema)
        {
            openApiSchema.Extensions ??= [];
            openApiSchema.Extensions.Add("X-foo", new OpenApiAny("bar"));
        }
    }
}
