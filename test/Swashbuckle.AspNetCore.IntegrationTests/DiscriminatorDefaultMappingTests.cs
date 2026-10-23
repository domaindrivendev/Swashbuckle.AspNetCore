using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using NJsonSchema;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Swashbuckle.AspNetCore.IntegrationTests;

public class DiscriminatorDefaultMappingTests
{
    private static readonly JsonSerializerOptions SerializerOptions = new() { TypeInfoResolver = new DefaultJsonTypeInfoResolver() };

    [Fact]
    public async Task PayloadWithoutDiscriminatorOnlyMatchesBaseTypeDefaultMapping()
    {
        // Arrange
        Assert.IsType<Shape>(JsonSerializer.Deserialize<Shape>("{}", SerializerOptions));
        Assert.IsType<Circle>(JsonSerializer.Deserialize<Shape>("""{"$type":"circle"}""", SerializerOptions));

        var validator = await CreateValidatorAsync<Shape>();

        // Act and Assert
        Assert.Equal(1, CountMatchingBranches(validator, "{}"));
        Assert.Empty(validator.Validate("{}"));
    }

    [Fact]
    public async Task PayloadWithoutDiscriminatorOnlyMatchesSubtypeDefaultMapping()
    {
        // Arrange
        var validator = await CreateValidatorAsync<Animal>();

        // Act and Assert
        Assert.Equal(1, CountMatchingBranches(validator, "{}"));
        Assert.Empty(validator.Validate("{}"));
    }

    private static int CountMatchingBranches(JsonSchema validator, string json)
        => validator.OneOf.Count((branch) => branch.Validate(json).Count == 0);

    private static async Task<JsonSchema> CreateValidatorAsync<T>()
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
        root["definitions"] = schemas.DeepClone();

        var json = root.ToJsonString().Replace("#/components/schemas/", "#/definitions/", StringComparison.Ordinal);

        return await JsonSchema.FromJsonAsync(json, TestContext.Current.CancellationToken);
    }

    [JsonPolymorphic(IgnoreUnrecognizedTypeDiscriminators = true)]
    [JsonDerivedType(typeof(Circle), "circle")]
    public class Shape;

    public class Circle : Shape;

    [SwaggerDiscriminator("kind", DefaultMapping = typeof(Cat))]
    [SwaggerSubType(typeof(Cat), DiscriminatorValue = "cat")]
    [SwaggerSubType(typeof(Dog), DiscriminatorValue = "dog")]
    public abstract class Animal;

    public class Cat : Animal;

    public class Dog : Animal;
}
