using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace DocumentationSnippets;

// begin-snippet: SwaggerGen-AutoRestSchemaFilter
public class AutoRestSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        var type = context.Type;
        if (type.IsEnum)
        {
            schema.Extensions.Add(
                "x-ms-enum",
                new OpenApiObject
                {
                    ["name"] = new OpenApiString(type.Name),
                    ["modelAsString"] = new OpenApiBoolean(true)
                }
            );
        }
    }
}
// end-snippet
