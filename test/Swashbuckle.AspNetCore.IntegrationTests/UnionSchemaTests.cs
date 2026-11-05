#if NET11_0_OR_GREATER
using System.Text.Json;

namespace Swashbuckle.AspNetCore.IntegrationTests;

public class UnionSchemaTests
{
    [Theory]
    [InlineData("null")]
    [InlineData("1")]
    [InlineData("[1,[2]]")]
    public async Task NullableRecursiveUnionAcceptsSerializedValues(string json)
    {
        // Arrange
        Assert.Equal(json, JsonSerializer.Serialize(JsonSerializer.Deserialize<RecursiveValue?>(json, SchemaValidator.SerializerOptions), SchemaValidator.SerializerOptions));

        var validator = await SchemaValidator.CreateAsync<RecursiveValue?>();

        // Act and Assert
        Assert.Empty(validator.Validate(json));
    }

    [Theory]
    [InlineData("null")]
    [InlineData("1")]
    [InlineData("\"foo\"")]
    public async Task NullableUnionAcceptsSerializedValues(string json)
    {
        // Arrange
        Assert.Equal(json, JsonSerializer.Serialize(JsonSerializer.Deserialize<ScalarValue?>(json, SchemaValidator.SerializerOptions), SchemaValidator.SerializerOptions));

        var validator = await SchemaValidator.CreateAsync<ScalarValue?>();

        // Act and Assert
        Assert.Empty(validator.Validate(json));
    }

    [Theory]
    [InlineData("null")]
    [InlineData("\"foo\"")]
    [InlineData("[\"foo\"]")]
    public async Task RecursiveUnionRejectsInvalidValues(string json)
    {
        // Arrange
        var validator = await SchemaValidator.CreateAsync<RecursiveValue>();

        // Act and Assert
        Assert.NotEmpty(validator.Validate(json));
    }

    public union RecursiveValue(int, RecursiveValue[]);

    public union ScalarValue(int, string);
}
#endif
