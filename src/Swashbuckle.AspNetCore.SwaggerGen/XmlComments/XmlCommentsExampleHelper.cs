using System.Text.Json;
#if NET10_0_OR_GREATER
using System.Text.Json.Nodes;
#endif
using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.SwaggerGen;

internal static class XmlCommentsExampleHelper
{
#if NET10_0_OR_GREATER
    public static JsonNode Create(
        SchemaRepository schemaRepository,
        OpenApiSchema schema,
        string exampleString)
    {
        var isStringType =
            schema?.ResolveType(schemaRepository) == JsonSchemaTypes.String &&
            !string.Equals(exampleString, "null");

        if (isStringType)
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
#else
    public static Microsoft.OpenApi.Any.IOpenApiAny Create(
        SchemaRepository schemaRepository,
        OpenApiSchema schema,
        string exampleString)
    {
        var isStringType =
            schema?.ResolveType(schemaRepository) == JsonSchemaTypes.String &&
            !string.Equals(exampleString, "null");

        var exampleAsJson = isStringType
            ? JsonSerializer.Serialize(exampleString)
            : exampleString;

        return JsonModelFactory.CreateFromJson(exampleAsJson);
    }
#endif
}
