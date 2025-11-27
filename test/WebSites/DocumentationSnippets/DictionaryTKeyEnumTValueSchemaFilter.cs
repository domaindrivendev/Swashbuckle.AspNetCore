using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace DocumentationSnippets;

// begin-snippet: SwaggerGen-DictionaryTKeyEnumTValueSchemaFilter
public class DictionaryTKeyEnumTValueSchemaFilter : ISchemaFilter
{
    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema is not OpenApiSchema concrete)
        {
            return;
        }

        // Only run for fields that are a Dictionary<Enum, TValue>
        if (!context.Type.IsGenericType || !context.Type.GetGenericTypeDefinition().IsAssignableFrom(typeof(Dictionary<,>)))
        {
            return;
        }

        var genericArgs = context.Type.GetGenericArguments();
        var keyType = genericArgs[0];
        var valueType = genericArgs[1];

        if (!keyType.IsEnum)
        {
            return;
        }

        concrete.Type = JsonSchemaType.Object;
        concrete.Properties = keyType.GetEnumNames().ToDictionary(
            name => name,
            name => context.SchemaGenerator.GenerateSchema(valueType, context.SchemaRepository));
    }
}
// end-snippet
