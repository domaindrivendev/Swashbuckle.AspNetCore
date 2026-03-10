using Microsoft.OpenApi;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test;

public static class SchemaRepositoryTests
{
    [Fact]
    public static void ReplaceSchemaId_ExistingSchema_ReplacesSchemaId()
    {
        var repository = new SchemaRepository();
        (string oldSchemaId, IOpenApiSchema exampleSchema) = GenerateSchemaForType(typeof(Example), repository);

        string newSchemaId = "alternateId";
        bool result = repository.ReplaceSchemaId(typeof(Example), newSchemaId);

        Assert.True(result);
        Assert.DoesNotContain(oldSchemaId, repository.Schemas);
        Assert.Contains(newSchemaId, repository.Schemas);
        Assert.Same(exampleSchema, repository.Schemas[newSchemaId]);
    }

    [Fact]
    public static void ReplaceSchemaId_MultipleSchemas_ReplacesSchemaId()
    {
        var repository = new SchemaRepository();
        (string oldSchemaId, IOpenApiSchema exampleSchema) = GenerateSchemaForType(typeof(Example), repository);
        (string otherSchemaId, IOpenApiSchema otherSchema) = GenerateSchemaForType(typeof(Exception), repository);

        string newSchemaId = "alternateId";
        bool result = repository.ReplaceSchemaId(typeof(Example), newSchemaId);

        Assert.True(result);
        Assert.DoesNotContain(oldSchemaId, repository.Schemas);
        Assert.Contains(newSchemaId, repository.Schemas);
        Assert.Same(exampleSchema, repository.Schemas[newSchemaId]);
        Assert.Contains(otherSchemaId, repository.Schemas);
        Assert.Same(otherSchema, repository.Schemas[otherSchemaId]);
    }

    [Fact]
    public static void ReplaceSchemaId_MissingSchema_DoesNotChange()
    {
        var repository = new SchemaRepository();

        string newSchemaId = "alternateId";
        bool result = repository.ReplaceSchemaId(typeof(Example), newSchemaId);

        Assert.False(result);
        Assert.DoesNotContain(newSchemaId, repository.Schemas);
    }

    [Fact]
    public static void ReplaceSchemaId_UnchangedId_DoesNotChange()
    {
        var repository = new SchemaRepository();
        (string schemaId, IOpenApiSchema exampleSchema) = GenerateSchemaForType(typeof(Example), repository);

        bool result = repository.ReplaceSchemaId(typeof(Example), schemaId);

        Assert.False(result);
        Assert.Contains(schemaId, repository.Schemas);
        Assert.Same(exampleSchema, repository.Schemas[schemaId]);
    }

    [Fact]
    public static void ReplaceSchemaId_ExistingId_DoesNotChange()
    {
        var repository = new SchemaRepository();

        var otherSchema = new OpenApiSchema();
        string otherSchemaId = "alreadyInUse";
        repository.Schemas.Add(otherSchemaId, otherSchema);

        GenerateSchemaForType(typeof(Example), repository);

        bool result = repository.ReplaceSchemaId(typeof(Example), otherSchemaId);

        Assert.False(result);
        Assert.Contains(otherSchemaId, repository.Schemas);
        Assert.Same(otherSchema, repository.Schemas[otherSchemaId]);
    }

    [Fact]
    public static void ReplaceSchemaId_CanGenerateMultipleTimes()
    {
        var repository = new SchemaRepository();
        GenerateSchemaForType(typeof(Example), repository);

        string newSchemaId1 = "exampleVariant1";
        bool result1 = repository.ReplaceSchemaId(typeof(Example), newSchemaId1);
        Assert.True(result1);

        GenerateSchemaForType(typeof(Example), repository);

        string newSchemaId2 = "exampleVariant2";
        bool result2 = repository.ReplaceSchemaId(typeof(Example), newSchemaId2);
        Assert.True(result2);

        Assert.Equal(2, repository.Schemas.Count);
        Assert.Contains(newSchemaId1, repository.Schemas);
        Assert.Contains(newSchemaId2, repository.Schemas);
    }

    private static (string, IOpenApiSchema) GenerateSchemaForType(Type type, SchemaRepository repository)
    {
        var generator = new SchemaGenerator(new(), new JsonSerializerDataContractResolver(new()));
        var schemaReference = (OpenApiSchemaReference)generator.GenerateSchema(type, repository);

        string schemaId = schemaReference.Reference.Id!;
        IOpenApiSchema schema = repository.Schemas[schemaId];

        return (schemaId, schema);
    }

    private sealed class Example
    {
        public string Value { get; set; }
    }
}
