using System.Text.Json.Nodes;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace DocumentationSnippets;

// begin-snippet: SwaggerGen-AutoRestSchemaFilter
public class AutoRestSchemaFilter : ISchemaFilter
{
    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        var type = context.Type;
        if (type.IsEnum && schema is OpenApiSchema concrete)
        {
            concrete.Extensions ??= new Dictionary<string, IOpenApiExtension>();
            concrete.Extensions.Add(
                "x-ms-enum",
                new JsonNodeExtension(
                    new JsonObject
                    {
                        ["name"] = type.Name,
                        ["modelAsString"] = true
                    }
                )
            );
        }
    }
}
// end-snippet
