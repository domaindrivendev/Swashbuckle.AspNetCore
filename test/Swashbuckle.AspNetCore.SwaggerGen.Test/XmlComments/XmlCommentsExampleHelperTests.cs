using System.Text.Json;
using Microsoft.OpenApi;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test;

public class XmlCommentsExampleHelperTests
{
    private readonly SchemaRepository schemaRepository = new();

    [Fact]
    public void Create_Builds_OpenApiArrayJson_When_Not_String_Type_And_Data_Is_Array()
    {
        var schema = new OpenApiSchema();

        var example = XmlCommentsExampleHelper.Create(
            schemaRepository,
            schema,
            "[\"one\",\"two\",\"three\"]");

        Assert.NotNull(example);

        Assert.Equal(JsonValueKind.Array, example.GetValueKind());
        var actual = example.AsArray();

        Assert.Equal(3, actual.Count);

        Assert.Equal(JsonValueKind.String, actual[0].GetValueKind());
        Assert.Equal(JsonValueKind.String, actual[1].GetValueKind());
        Assert.Equal(JsonValueKind.String, actual[2].GetValueKind());
        Assert.Equal("one", actual[0].GetValue<string>());
        Assert.Equal("two", actual[1].GetValue<string>());
        Assert.Equal("three", actual[2].GetValue<string>());
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("foo")]
    [InlineData("example string with special characters\"<>\r\n\"")]
    public void Create_Builds_OpenApiString_When_Type_String(string exampleString)
    {
        var schema = new OpenApiSchema { Type = JsonSchemaTypes.String };
        schemaRepository.AddDefinition("test", schema);

        var example = XmlCommentsExampleHelper.Create(
            schemaRepository, schema, exampleString);

        Assert.NotNull(example);

        Assert.Equal(JsonValueKind.String, example.GetValueKind());
        Assert.Equal(exampleString, example.GetValue<string>());
    }

    [Fact]
    public void Create_Returns_Null_When_Type_String_And_Value_Is_Null()
    {
        var schema = new OpenApiSchema { Type = JsonSchemaTypes.String };
        schemaRepository.AddDefinition("test", schema);

        var example = XmlCommentsExampleHelper.Create(
            schemaRepository, schema, null);

        Assert.Null(example);
    }

    [Fact]
    public void Create_Returns_Null_When_Value_And_Schema_Are_Null()
    {
        var example = XmlCommentsExampleHelper.Create(schemaRepository, null, null);

        Assert.Null(example);
    }

    [Fact]
    public void Create_Returns_Null_When_Type_String_And_Value_Null_String_Literal()
    {
        var schema = new OpenApiSchema { Type = JsonSchemaTypes.String };
        schemaRepository.AddDefinition("test", schema);

        var example = XmlCommentsExampleHelper.Create(
            schemaRepository, schema, "null");

        Assert.Null(example);
    }

    [Fact]
    public void Create_Allows_Schema_To_Be_Null()
    {
        OpenApiSchema schema = null;

        var example = XmlCommentsExampleHelper.Create(schemaRepository, schema, "[]");

        Assert.NotNull(example);

        Assert.Equal(JsonValueKind.Array, example.GetValueKind());
        Assert.Empty(example.AsArray());
    }

    [Fact]
    public void Create_Builds_OpenApiString_When_Type_Integer()
    {
        string exampleString = "1";
        var schema = new OpenApiSchema { Type = JsonSchemaTypes.Integer };
        schemaRepository.AddDefinition("test", schema);

        var example = XmlCommentsExampleHelper.Create(
            schemaRepository, schema, exampleString);

        Assert.NotNull(example);
        Assert.Equal(1, example.GetValue<int>());
    }

    [Fact]
    public void Create_Returns_Null_When_Type_Integer_And_Value_Is_Null()
    {
        var schema = new OpenApiSchema { Type = JsonSchemaTypes.Integer };
        schemaRepository.AddDefinition("test", schema);

        var example = XmlCommentsExampleHelper.Create(
            schemaRepository, schema, null);

        Assert.Null(example);
    }
}
