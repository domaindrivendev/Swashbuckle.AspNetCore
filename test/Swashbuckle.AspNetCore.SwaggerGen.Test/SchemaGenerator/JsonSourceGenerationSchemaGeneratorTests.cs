using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Microsoft.OpenApi;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test;

public class JsonSourceGenerationSchemaGeneratorTests
{
    [Fact]
    public void GenerateSchema_GeneratesEnumValues_ForIntegerEnumResolvedBySourceGeneration()
    {
        var schema = GenerateSchemaFor(typeof(SourceGeneratedTimeRange));

        Assert.Equal(JsonSchemaTypes.Integer, schema.Type);
        Assert.Equal(["0", "1", "2"], schema.Enum.Select((p) => p.ToJsonString()));
    }

    [Fact]
    public void GenerateSchema_GeneratesEnumValues_ForNullableIntegerEnumResolvedBySourceGeneration()
    {
        var schema = GenerateSchemaFor(typeof(SourceGeneratedTimeRange?));

        Assert.Equal(JsonSchemaTypes.Integer, schema.Type);
        Assert.Equal(["0", "1", "2"], schema.Enum.Select((p) => p.ToJsonString()));
    }

    [Fact]
    public void GenerateSchema_GeneratesEnumValues_ForStringEnumResolvedBySourceGeneration()
    {
        var schema = GenerateSchemaFor(typeof(SourceGeneratedLevel));

        Assert.Equal(JsonSchemaTypes.String, schema.Type);
        Assert.Equal(["\"Low\"", "\"High\""], schema.Enum.Select((p) => p.ToJsonString()));
    }

    [Fact]
    public void GenerateSchema_GeneratesEnumValues_ForEnumPropertyResolvedBySourceGeneration()
    {
        var repository = new SchemaRepository();

        Subject().GenerateSchema(typeof(SourceGeneratedForecast), repository);

        var schema = Assert.IsType<OpenApiSchema>(repository.Schemas[nameof(SourceGeneratedTimeRange)]);

        Assert.Equal(JsonSchemaTypes.Integer, schema.Type);
        Assert.Equal(["0", "1", "2"], schema.Enum.Select((p) => p.ToJsonString()));
    }

    private static OpenApiSchema GenerateSchemaFor(Type type)
    {
        var repository = new SchemaRepository();

        Subject().GenerateSchema(type, repository);

        var name = (Nullable.GetUnderlyingType(type) ?? type).Name;

        return Assert.IsType<OpenApiSchema>(repository.Schemas[name]);
    }

    private static SchemaGenerator Subject()
    {
        // Reproduces an application that has disabled reflection-based serialization
        // by only resolving JSON metadata through a source-generated context.
        var serializerOptions = new JsonSerializerOptions()
        {
            TypeInfoResolver = SourceGenerationContext.Default,
        };

        var generatorOptions = new SchemaGeneratorOptions();

        return new SchemaGenerator(generatorOptions, new JsonSerializerDataContractResolver(serializerOptions, generatorOptions));
    }
}

public enum SourceGeneratedTimeRange
{
    Daily,
    Weekly,
    Monthly,
}

[JsonConverter(typeof(JsonStringEnumConverter<SourceGeneratedLevel>))]
public enum SourceGeneratedLevel
{
    Low,
    High,
}

public class SourceGeneratedForecast
{
    public SourceGeneratedTimeRange Range { get; set; }

    public int TemperatureC { get; set; }
}

[JsonSerializable(typeof(SourceGeneratedTimeRange))]
[JsonSerializable(typeof(SourceGeneratedLevel))]
[JsonSerializable(typeof(SourceGeneratedForecast))]
internal sealed partial class SourceGenerationContext : JsonSerializerContext;
