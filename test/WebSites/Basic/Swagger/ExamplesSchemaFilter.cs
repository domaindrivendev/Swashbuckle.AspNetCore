#if NET10_0_OR_GREATER
using System.Text.Json.Nodes;
#else
using Microsoft.OpenApi.Any;
#endif
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Basic.Swagger
{
    public class ExamplesSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
#if NET10_0_OR_GREATER
            schema.Example = context.Type.Name switch
            {
                "Product" => new JsonObject
                {
                    ["id"] = 123,
                    ["description"] = "foobar",
                    ["price"] = 14.37d
                },
                _ => null,
            };
#else
            schema.Example = context.Type.Name switch
            {
                "Product" => new OpenApiObject
                {
                    ["id"] = new OpenApiInteger(123),
                    ["description"] = new OpenApiString("foobar"),
                    ["price"] = new OpenApiDouble(14.37)
                },
                _ => null
            };
#endif
        }
    }
}
