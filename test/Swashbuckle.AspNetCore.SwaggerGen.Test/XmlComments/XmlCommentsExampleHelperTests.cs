using System.Text.Json;
using Microsoft.OpenApi.Models;
using Xunit;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    public class XmlCommentsExampleHelperTests
    {
        private readonly SchemaRepository schemaRepository = new();

        [Fact]
        public void Create_BuildsOpenApiArrayJson__When_NotStringTypeAndDataIsArray()
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
            Assert.Equal("one",   actual[0].GetValue<string>());
            Assert.Equal("two",   actual[1].GetValue<string>());
            Assert.Equal("three", actual[2].GetValue<string>());
        }

        [Fact]
        public void Create_BuildsOpenApiString_When_TypeString()
        {
            string exampleString = "example string with special characters\"<>\r\n\"";
            var schema = new OpenApiSchema { Type = JsonSchemaType.String };
            schemaRepository.AddDefinition("test", schema);

            var example = XmlCommentsExampleHelper.Create(
                schemaRepository, schema, exampleString);

            Assert.NotNull(example);
            Assert.Equal(JsonValueKind.String, example.GetValueKind());
            Assert.Equal(exampleString, example.GetValue<string>());
        }

        [Fact]
        public void Create_ReturnsNull_When_TypeString_and_ValueNull()
        {
            var schema = new OpenApiSchema { Type = JsonSchemaType.String };
            schemaRepository.AddDefinition("test", schema);

            var example = XmlCommentsExampleHelper.Create(
                schemaRepository, schema, "null");

            Assert.NotNull(example);
            Assert.Equal(JsonValueKind.Null, example.GetValueKind());
        }

        [Fact]
        public void Create_AllowsSchemaToBeNull()
        {
            OpenApiSchema schema = null;

            var example = XmlCommentsExampleHelper.Create(schemaRepository, schema, "[]");

            Assert.NotNull(example);
            Assert.Equal(JsonValueKind.Array, example.GetValueKind());
            Assert.Empty(example.AsArray());
        }
    }
}
