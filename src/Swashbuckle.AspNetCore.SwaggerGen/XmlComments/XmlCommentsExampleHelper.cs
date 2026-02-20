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


        if (string.Equals(exampleString, "null"))
        {
            return (type?.HasFlag(JsonSchemaType.Null) ?? false) ? JsonNullSentinel.JsonNull : null;
        }

        if (type is { } value && value.HasFlag(JsonSchemaType.String))
        {
            return JsonValue.Create(exampleString);
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
