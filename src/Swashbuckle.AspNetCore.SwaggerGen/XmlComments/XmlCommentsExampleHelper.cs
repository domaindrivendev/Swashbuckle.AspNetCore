using System.Text.Json;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public static class XmlCommentsExampleHelper
    {
        public static IOpenApiAny Create(
            SchemaRepository schemaRepository,
            OpenApiSchema schema,
            string exampleString)
        {
            var isStringType =
                (schema?.ResolveType(schemaRepository) == "string") &&
                !exampleString.Equals("null");

            var exampleAsJson = isStringType
                    ? JsonSerializer.Serialize(exampleString)
                    : exampleString;

            var example = OpenApiAnyFactory.CreateFromJson(exampleAsJson);

            return example;
        }
    }
}
