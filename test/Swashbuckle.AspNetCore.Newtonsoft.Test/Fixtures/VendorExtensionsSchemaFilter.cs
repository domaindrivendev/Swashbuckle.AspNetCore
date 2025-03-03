using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Swashbuckle.AspNetCore.Newtonsoft.Test
{
    public class VendorExtensionsSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
#if NET10_0_OR_GREATER
            schema.Extensions.Add("X-foo", new OpenApiAny("bar"));
#else
            schema.Extensions.Add("X-foo", new OpenApiString("bar"));
#endif
        }
    }
}
