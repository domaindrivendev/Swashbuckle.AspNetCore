using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Swashbuckle.AspNetCore.Annotations.Test
{
    public class VendorExtensionsSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
#if NET10_0_OR_GREATER
            schema.Extensions.Add("X-property1", new OpenApiAny("value"));
#else
            schema.Extensions.Add("X-property1", new OpenApiString("value"));
#endif
        }
    }
}
