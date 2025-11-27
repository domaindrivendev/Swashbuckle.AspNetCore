using System.Text.Json.Nodes;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Basic.Swagger;

public class ExamplesSchemaFilter : ISchemaFilter
{
    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema is not OpenApiSchema concrete)
        {
            return;
        }

        concrete.Example = context.Type.Name switch
        {
            "Product" => new JsonObject
            {
                ["id"] = 123,
                ["description"] = "foobar",
                ["price"] = 14.37d
            },
            _ => null,
        };
    }
}
