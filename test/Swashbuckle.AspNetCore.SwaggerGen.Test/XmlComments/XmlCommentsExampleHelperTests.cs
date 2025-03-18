#if NET10_0_OR_GREATER
using System.Text.Json;
#else
using Microsoft.OpenApi.Any;
#endif
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

#if NET10_0_OR_GREATER
            Assert.Equal(JsonValueKind.Array, example.GetValueKind());
            var actual = example.AsArray();
#else
            var actual = Assert.IsType<OpenApiArray>(example);
#endif

            Assert.Equal(3, actual.Count);

#if NET10_0_OR_GREATER
            Assert.Equal(JsonValueKind.String, actual[0].GetValueKind());
            Assert.Equal(JsonValueKind.String, actual[1].GetValueKind());
            Assert.Equal(JsonValueKind.String, actual[2].GetValueKind());
            Assert.Equal("one",   actual[0].GetValue<string>());
            Assert.Equal("two",   actual[1].GetValue<string>());
            Assert.Equal("three", actual[2].GetValue<string>());
#else
            var item1 = Assert.IsType<OpenApiString>(actual[0]);
            var item2 = Assert.IsType<OpenApiString>(actual[1]);
            var item3 = Assert.IsType<OpenApiString>(actual[2]);
            Assert.Equal("one", item1.Value);
            Assert.Equal("two", item2.Value);
            Assert.Equal("three", item3.Value);
#endif
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

#if NET10_0_OR_GREATER
            Assert.Equal(JsonValueKind.String, example.GetValueKind());
            Assert.Equal(exampleString, example.GetValue<string>());
#else
            var actual = Assert.IsType<OpenApiString>(example);
            Assert.Equal(actual.Value, exampleString);
#endif
        }

        [Fact]
        public void Create_ReturnsNull_When_TypeString_and_ValueNull()
        {
            var schema = new OpenApiSchema { Type = JsonSchemaTypes.String };
            schemaRepository.AddDefinition("test", schema);

            var example = XmlCommentsExampleHelper.Create(
                schemaRepository, schema, "null");

#if NET10_0_OR_GREATER
            Assert.Null(example);
#else
            Assert.NotNull(example);
            var actual = Assert.IsType<OpenApiNull>(example);
            Assert.Equal(AnyType.Null, actual.AnyType);
#endif
        }

        [Fact]
        public void Create_AllowsSchemaToBeNull()
        {
            OpenApiSchema schema = null;

            var example = XmlCommentsExampleHelper.Create(schemaRepository, schema, "[]");

            Assert.NotNull(example);

#if NET10_0_OR_GREATER
            Assert.Equal(JsonValueKind.Array, example.GetValueKind());
            Assert.Empty(example.AsArray());
#else
            var actual = Assert.IsType<OpenApiArray>(example);
            Assert.Empty(actual);
#endif
        }
    }
}
