using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models.Interfaces;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Swashbuckle.AspNetCore.Newtonsoft.Test;

public class VendorExtensionsSchemaFilter : ISchemaFilter
{
    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        schema.Extensions.Add("X-foo", new OpenApiAny("bar"));
    }
}
