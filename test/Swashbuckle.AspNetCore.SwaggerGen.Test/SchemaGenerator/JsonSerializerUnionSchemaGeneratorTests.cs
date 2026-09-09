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
}
#endif
