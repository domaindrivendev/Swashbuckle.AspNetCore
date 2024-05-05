using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Xunit;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    public class XmlCommentsExampleHelperTests
    {
        private readonly SchemaRepository schemaRepository = new SchemaRepository();

        [Fact]
        public void Create_BuildsOpenApiArrayJson__When_NotStringTypeAndDataIsArray()
        {
            var schema = new OpenApiSchema();

            IOpenApiAny example = XmlCommentsExampleHelper.Create(
                schemaRepository,
                schema,
                "[\"one\",\"two\",\"three\"]");


            Assert.NotNull(example);
            Assert.IsType<OpenApiArray>(example);
            var output = (OpenApiArray)example;
            Assert.Equal(3, output.Count);
            Assert.Equal("one",   ((OpenApiString)output[0]).Value);
            Assert.Equal("two",   ((OpenApiString)output[1]).Value);
            Assert.Equal("three", ((OpenApiString)output[2]).Value);
        }

        [Fact]
        public void Create_BuildsOpenApiString_When_TypeString()
        {
            string exampleString = "example string with special characters\"<>\r\n\"";
            OpenApiSchema schema = new OpenApiSchema { Type = "string" };
            schemaRepository.AddDefinition("test", schema);
            
            IOpenApiAny example = XmlCommentsExampleHelper.Create(
                schemaRepository, schema, exampleString);

            Assert.NotNull(example);
            Assert.IsType<OpenApiString>(example);
            Assert.Equal(((OpenApiString)example).Value, exampleString);
        }

        [Fact]
        public void Create_ReturnsNull_When_TypeString_and_ValueNull()
        {
            OpenApiSchema schema = new OpenApiSchema { Type = "string" };
            schemaRepository.AddDefinition("test", schema);

            IOpenApiAny example = XmlCommentsExampleHelper.Create(
                schemaRepository, schema, "null");

            Assert.NotNull(example);
            Assert.IsType<OpenApiNull>(example);
            Assert.Equal(AnyType.Null, ((OpenApiNull)example).AnyType);
        }

        [Fact]
        public void Create_AllowsSchemaToBeNull()
        {
            OpenApiSchema schema = null;

            IOpenApiAny example = XmlCommentsExampleHelper.Create(schemaRepository, schema, "[]");

            Assert.NotNull(example);
            Assert.IsType<OpenApiArray>(example);
            Assert.Empty((OpenApiArray)example);
        }
    }
}
