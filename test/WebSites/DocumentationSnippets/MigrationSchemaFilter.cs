using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace DocumentationSnippets;

public class MigrationSchemaFilter : ISchemaFilter
{
    // begin-snippet: Migrating-SchemaFilter
    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema is OpenApiSchema openApiSchema)
        {
            // The properties are only mutable on the concrete type
            openApiSchema.Type = JsonSchemaType.String;
        }
    }
    // end-snippet
}
