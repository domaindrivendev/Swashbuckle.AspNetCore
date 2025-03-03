using Microsoft.OpenApi.Any;
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

            var actual = Assert.IsType<OpenApiArray>(example);

            Assert.Equal(3, actual.Count);

            var item1 = Assert.IsType<OpenApiString>(actual[0]);
            var item2 = Assert.IsType<OpenApiString>(actual[1]);
            var item3 = Assert.IsType<OpenApiString>(actual[2]);
            Assert.Equal("one", item1.Value);
            Assert.Equal("two", item2.Value);
            Assert.Equal("three", item3.Value);
        }

        [Fact]
        public void Create_BuildsOpenApiString_When_TypeString()
        {
            string exampleString = "example string with special characters\"<>\r\n\"";
            var schema = new OpenApiSchema { Type = JsonSchemaTypes.String };
            schemaRepository.AddDefinition("test", schema);

            var example = XmlCommentsExampleHelper.Create(
                schemaRepository, schema, exampleString);

            Assert.NotNull(example);

            var actual = Assert.IsType<OpenApiString>(example);
            Assert.Equal(actual.Value, exampleString);
        }

        [Fact]
        public void Create_ReturnsNull_When_TypeString_and_ValueNull()
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
        public void Create_AllowsSchemaToBeNull()
        {
            OpenApiSchema schema = null;

            var example = XmlCommentsExampleHelper.Create(schemaRepository, schema, "[]");

            Assert.NotNull(example);

            var actual = Assert.IsType<OpenApiArray>(example);
            Assert.Empty(actual);
        }
    }
}
