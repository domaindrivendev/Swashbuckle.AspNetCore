using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace DocumentationSnippets;

// begin-snippet: Annotations-SchemaFilter
public class ItemSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        schema.Example = new OpenApiObject
        {
            ["Id"] = new OpenApiInteger(1),
            ["Description"] = new OpenApiString("An awesome item")
        };
    }
}
// end-snippet
