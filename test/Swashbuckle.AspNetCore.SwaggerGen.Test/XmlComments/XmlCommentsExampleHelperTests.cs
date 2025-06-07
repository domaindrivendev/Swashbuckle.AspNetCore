using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

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

        var actual = Assert.IsType<OpenApiArray>(example);

        Assert.Equal(3, actual.Count);

        var item1 = Assert.IsType<OpenApiString>(actual[0]);
        var item2 = Assert.IsType<OpenApiString>(actual[1]);
        var item3 = Assert.IsType<OpenApiString>(actual[2]);
        Assert.Equal("one", item1.Value);
        Assert.Equal("two", item2.Value);
        Assert.Equal("three", item3.Value);
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

        var actual = Assert.IsType<OpenApiString>(example);
        Assert.Equal(actual.Value, exampleString);
    }

    [Fact]
    public void Create_Returns_Null_When_Type_String_And_Value_Is_Null()
    {
        var schema = new OpenApiSchema { Type = JsonSchemaTypes.String };
        schemaRepository.AddDefinition("test", schema);

        var example = XmlCommentsExampleHelper.Create(
            schemaRepository, schema, null);

        Assert.NotNull(example);

        var actual = Assert.IsType<OpenApiNull>(example);
        Assert.Equal(AnyType.Null, actual.AnyType);
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

        Assert.NotNull(example);

        var actual = Assert.IsType<OpenApiNull>(example);
        Assert.Equal(AnyType.Null, actual.AnyType);
    }

    [Fact]
    public void Create_Allows_Schema_To_Be_Null()
    {
        OpenApiSchema schema = null;

        var example = XmlCommentsExampleHelper.Create(schemaRepository, schema, "[]");

        Assert.NotNull(example);

        var actual = Assert.IsType<OpenApiArray>(example);
        Assert.Empty(actual);
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

        var actual = Assert.IsType<OpenApiInteger>(example);
        Assert.Equal(1, actual.Value);
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
