using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    public class TestSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            schema.Extensions.Add("X-foo", new OpenApiAny("bar"));
            schema.Extensions.Add("X-docName", new OpenApiAny(context.DocumentName));
        }
    }
}
