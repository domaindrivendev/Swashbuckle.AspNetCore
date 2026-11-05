#if NET11_0_OR_GREATER
#nullable enable
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Microsoft.OpenApi;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test;

public class JsonSerializerUnionSchemaGeneratorTests
{
    private static readonly Action<JsonSerializerOptions> UseReflectionResolver =
        static o => o.TypeInfoResolver = new DefaultJsonTypeInfoResolver();

    [Fact]
    public void GenerateSchema_GeneratesAnyOfSchema_ForUnionType()
    {
        var schemaRepository = new SchemaRepository();

        var schema = Subject(configureSerializer: UseReflectionResolver).GenerateSchema(typeof(Pet), schemaRepository);

        var concrete = Assert.IsType<OpenApiSchema>(schema);
        Assert.NotNull(concrete.AnyOf);
        Assert.Equal(2, concrete.AnyOf.Count);

        var references = concrete.AnyOf.Select(s => Assert.IsType<OpenApiSchemaReference>(s)).ToArray();
        var referencedIds = references.Select(r => r.Reference.Id).ToArray();

        Assert.Contains(nameof(Cat), referencedIds);
        Assert.Contains(nameof(Dog), referencedIds);

        // Each case type schema is registered in the repository.
        Assert.True(schemaRepository.Schemas.ContainsKey(nameof(Cat)));
        Assert.True(schemaRepository.Schemas.ContainsKey(nameof(Dog)));
    }

    [Fact]
    public void GenerateSchema_GeneratesAnyOfSchema_ForUnionTypeProperty()
    {
        var schemaRepository = new SchemaRepository();

        Subject(configureSerializer: UseReflectionResolver).GenerateSchema(typeof(Envelope), schemaRepository);

        var schema = schemaRepository.Schemas[nameof(Envelope)];
        Assert.NotNull(schema.Properties);

        var property = Assert.IsType<OpenApiSchema>(schema.Properties[nameof(Envelope.Value)]);

        Assert.NotNull(property.AnyOf);
        Assert.Equal(2, property.AnyOf.Count);

        var referencedIds = property.AnyOf.Select(s => Assert.IsType<OpenApiSchemaReference>(s).Reference.Id).ToArray();

        Assert.Contains(nameof(Cat), referencedIds);
        Assert.Contains(nameof(Dog), referencedIds);
    }

    [Fact]
    public void GenerateSchema_GeneratesAnyOfSchema_ForUnionTypeArrayItems()
    {
        var schemaRepository = new SchemaRepository();

        var schema = Subject(configureSerializer: UseReflectionResolver).GenerateSchema(typeof(Pet[]), schemaRepository);

        var concrete = Assert.IsType<OpenApiSchema>(schema);
        var items = Assert.IsType<OpenApiSchema>(concrete.Items);

        Assert.NotNull(items.AnyOf);
        Assert.Equal(2, items.AnyOf.Count);
    }

    [Fact]
    public void GenerateSchema_CustomTypeMapping_TakesPrecedenceOverUnionType()
    {
        var expected = new OpenApiSchema { Type = JsonSchemaType.String, Format = "custom" };

        var subject = Subject(
            configureGenerator: (c) => c.CustomTypeMappings.Add(typeof(Pet), () => expected),
            configureSerializer: UseReflectionResolver);

        var schema = subject.GenerateSchema(typeof(Pet), new SchemaRepository());

        Assert.Same(expected, schema);
    }

    [Fact]
    public void GenerateSchema_GeneratesReferencedAnyOfSchema_ForRecursiveUnionType()
    {
        var serializerOptions = new JsonSerializerOptions();
        UseReflectionResolver(serializerOptions);

        RecursiveValue value = new RecursiveValue[] { 1, new RecursiveValue[] { 2 } };
        Assert.Equal("[1,[2]]", JsonSerializer.Serialize(value, serializerOptions));

        var schemaRepository = new SchemaRepository();

        var schema = Subject(configureSerializer: UseReflectionResolver).GenerateSchema(typeof(RecursiveValue), schemaRepository);

        AssertIsUnionReference(schema, nullable: false);

        var definition = Assert.IsType<OpenApiSchema>(schemaRepository.Schemas[nameof(RecursiveValue)]);
        Assert.NotNull(definition.AnyOf);
        Assert.Equal(2, definition.AnyOf.Count);

        var integerCase = Assert.IsType<OpenApiSchema>(definition.AnyOf[0]);
        Assert.Equal(JsonSchemaType.Integer, integerCase.Type);

        var arrayCase = Assert.IsType<OpenApiSchema>(definition.AnyOf[1]);
        Assert.Equal(JsonSchemaType.Array, arrayCase.Type);

        AssertIsUnionReference(arrayCase.Items, nullable: false);

        // Subsequent usages reference the same component
        var other = Subject(configureSerializer: UseReflectionResolver).GenerateSchema(typeof(RecursiveValue), schemaRepository);
        AssertIsUnionReference(other, nullable: false);
        Assert.Single(schemaRepository.Schemas);
    }

    [Fact]
    public void GenerateSchema_GeneratesNullableSchema_ForNullableRecursiveUnionType()
    {
        var schemaRepository = new SchemaRepository();

        var schema = Subject(configureSerializer: UseReflectionResolver).GenerateSchema(typeof(RecursiveValue?), schemaRepository);

        AssertIsUnionReference(schema, nullable: true);

        // The component itself is not nullable
        var definition = Assert.IsType<OpenApiSchema>(schemaRepository.Schemas[nameof(RecursiveValue)]);
        Assert.NotNull(definition.AnyOf);
        Assert.DoesNotContain(definition.AnyOf, (s) => s.Type == JsonSchemaType.Null);
    }

    [Fact]
    public void GenerateSchema_GeneratesNullableSchema_ForNullableRecursiveUnionTypeProperty()
    {
        var schemaRepository = new SchemaRepository();

        Subject(configureSerializer: UseReflectionResolver).GenerateSchema(typeof(RecursiveEnvelope), schemaRepository);

        var schema = schemaRepository.Schemas[nameof(RecursiveEnvelope)];
        Assert.NotNull(schema.Properties);

        AssertIsUnionReference(schema.Properties[nameof(RecursiveEnvelope.Value)], nullable: false);
        AssertIsUnionReference(schema.Properties[nameof(RecursiveEnvelope.NullableValue)], nullable: true);
    }

    private static void AssertIsUnionReference(IOpenApiSchema? schema, bool nullable)
    {
        var concrete = Assert.IsType<OpenApiSchema>(schema);
        Assert.NotNull(concrete.AnyOf);
        Assert.Equal(nullable ? 2 : 1, concrete.AnyOf.Count);

        var reference = Assert.IsType<OpenApiSchemaReference>(concrete.AnyOf[0]);
        Assert.Equal(nameof(RecursiveValue), reference.Reference.Id);

        if (nullable)
        {
            Assert.Equal(JsonSchemaType.Null, concrete.AnyOf[1].Type);
        }
    }

    private static SchemaGenerator Subject(
        Action<SchemaGeneratorOptions>? configureGenerator = null,
        Action<JsonSerializerOptions>? configureSerializer = null)
    {
        var generatorOptions = new SchemaGeneratorOptions();
        configureGenerator?.Invoke(generatorOptions);

        var serializerOptions = new JsonSerializerOptions();
        configureSerializer?.Invoke(serializerOptions);

        return new SchemaGenerator(generatorOptions, new JsonSerializerDataContractResolver(serializerOptions, generatorOptions));
    }

    public record Cat(string Name, int Lives);

    public record Dog(string Name, bool GoodBoy);

    public union Pet(Cat, Dog);

    public record Envelope(Pet Value);

    public union RecursiveValue(int, RecursiveValue[]);

    public record RecursiveEnvelope(RecursiveValue Value, RecursiveValue? NullableValue);
}
#endif
