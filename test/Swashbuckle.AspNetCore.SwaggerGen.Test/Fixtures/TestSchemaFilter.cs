using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    public class TestSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
#if NET10_0_OR_GREATER
            schema.Extensions.Add("X-foo", new OpenApiAny("bar"));
            schema.Extensions.Add("X-docName", new OpenApiAny(context.DocumentName));
#else
            schema.Extensions.Add("X-foo", new OpenApiString("bar"));
            schema.Extensions.Add("X-docName", new OpenApiString(context.DocumentName));
#endif
        }
    }
}
