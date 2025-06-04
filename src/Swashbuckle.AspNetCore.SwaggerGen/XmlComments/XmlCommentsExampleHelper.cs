using System.Text.Json;
using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.SwaggerGen;

internal static class XmlCommentsExampleHelper
{
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
}
