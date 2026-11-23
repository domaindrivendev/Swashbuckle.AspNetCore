using Microsoft.OpenApi;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test;

public class RecordingSchemaFilter : ISchemaFilter
{
    public Dictionary<IOpenApiSchema, Type> ContextTypes { get; } = new(ReferenceEqualityComparer.Instance);

    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        ContextTypes[schema] = context.Type;
    }
}
