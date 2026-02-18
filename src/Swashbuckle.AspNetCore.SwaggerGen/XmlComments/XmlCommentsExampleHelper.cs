using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.OpenApi;

namespace Swashbuckle.AspNetCore.SwaggerGen;

internal static class XmlCommentsExampleHelper
{
    public static JsonNode Create(
        SchemaRepository schemaRepository,
        IOpenApiSchema schema,
        string exampleString)
    {
        var type = schema?.ResolveType(schemaRepository);
        if (type is { } schemaType)
        {
            var isStringType = schemaType.HasFlag(JsonSchemaType.String);
            var nullable = schemaType.HasFlag(JsonSchemaType.Null);

            if (isStringType)
            {
                return nullable && string.Equals(exampleString, "null")
                    ? JsonNullSentinel.JsonNull
                    : JsonValue.Create(exampleString);
            }
        }

        // HACK If the value is a string, but we can't detect it as one, then
        // if parsing it fails, assume it is a string that isn't quoted. There's
        // probably a much better way to deal with this scenario.
        try
        {
            return JsonModelFactory.CreateFromJson(exampleString);
        }
        catch (JsonException) when (exampleString?.StartsWith('"') == false)
        {
            return JsonValue.Create(exampleString);
        }
    }
}
