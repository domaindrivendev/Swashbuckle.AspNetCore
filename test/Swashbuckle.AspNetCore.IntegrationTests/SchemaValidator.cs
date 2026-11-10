using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using NJsonSchema;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Swashbuckle.AspNetCore.IntegrationTests;

internal static class SchemaValidator
{
    public static readonly JsonSerializerOptions SerializerOptions = new() { TypeInfoResolver = new DefaultJsonTypeInfoResolver() };

    public static async Task<JsonSchema> CreateAsync<T>()
    {
        var options = new SwaggerGenOptions();
        options.EnableAnnotations(enableAnnotationsForInheritance: false, enableAnnotationsForPolymorphism: true);

        var generator = new SchemaGenerator(
            options.SchemaGeneratorOptions,
            new JsonSerializerDataContractResolver(SerializerOptions, options.SchemaGeneratorOptions));

        var repository = new SchemaRepository();
        var schema = generator.GenerateSchema(typeof(T), repository);

        var document = new OpenApiDocument
        {
            Info = new() { Title = "Test", Version = "v1" },
            Paths = [],
            Components = new() { Schemas = new Dictionary<string, IOpenApiSchema>(repository.Schemas) { ["Root"] = schema } },
        };

        using var writer = new StringWriter();
        document.SerializeAs(OpenApiSpecVersion.OpenApi3_2, new OpenApiJsonWriter(writer));

        var schemas = JsonNode.Parse(writer.ToString())["components"]["schemas"];
        var root = schemas["Root"].DeepClone().AsObject();

        if (root.ContainsKey("$ref"))
        {
            // Wrap a reference so that the definitions are not ignored alongside it
            root = new JsonObject { ["allOf"] = new JsonArray(root) };
        }

        root["definitions"] = schemas.DeepClone();

        var json = root.ToJsonString().Replace("#/components/schemas/", "#/definitions/", StringComparison.Ordinal);

        return await JsonSchema.FromJsonAsync(json, TestContext.Current.CancellationToken);
    }

    public static int CountMatchingAlternatives(JsonSchema schema, string json)
        => schema.OneOf.Count((alternative) => alternative.Validate(json).Count == 0);
}
