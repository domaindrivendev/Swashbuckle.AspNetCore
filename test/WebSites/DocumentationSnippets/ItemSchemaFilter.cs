using System.Text.Json.Nodes;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace DocumentationSnippets;

// begin-snippet: Annotations-SchemaFilter
public class ItemSchemaFilter : ISchemaFilter
{
    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema is OpenApiSchema concrete)
        {
            concrete.Example = new JsonObject
            {
                ["Id"] = 1,
                ["Description"] = "An awesome item"
            };
        }
    }
}
// end-snippet
