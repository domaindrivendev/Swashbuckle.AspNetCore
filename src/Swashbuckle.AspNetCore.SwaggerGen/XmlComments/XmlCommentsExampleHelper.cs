using System.Text.Json;
using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    internal static class XmlCommentsExampleHelper
    {
#if NET10_0_OR_GREATER
        public static System.Text.Json.Nodes.JsonNode Create(
#else
        public static Microsoft.OpenApi.Any.IOpenApiAny Create(
#endif
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
}
