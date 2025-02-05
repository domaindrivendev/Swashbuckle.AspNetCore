using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    internal static class XmlCommentsExampleHelper
    {
        public static JsonNode Create(
            SchemaRepository schemaRepository,
            OpenApiSchema schema,
            string exampleString)
        {
            var isStringType =
                schema?.ResolveType(schemaRepository) == JsonSchemaType.String &&
                !string.Equals(exampleString, "null");

            var exampleAsJson = isStringType
                    ? JsonSerializer.Serialize(exampleString)
                    : exampleString;

            var example = OpenApiAnyFactory.CreateFromJson(exampleAsJson);

            return example;
        }
    }
}
