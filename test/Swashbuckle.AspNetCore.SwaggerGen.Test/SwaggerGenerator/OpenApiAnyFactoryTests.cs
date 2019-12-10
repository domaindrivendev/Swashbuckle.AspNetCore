using System;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Xunit;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    public class OpenApiAnyFactoryTests
    {
        [Theory]
        [InlineData("boolean", null, true, typeof(OpenApiBoolean))]
        [InlineData("integer", "int32", (byte)10, typeof(OpenApiInteger))]
        [InlineData("integer", "int32", ByteEnum.Value2, typeof(OpenApiInteger))]
        [InlineData("integer", "int32", (short)10, typeof(OpenApiInteger))]
        [InlineData("integer", "int32", ShortEnum.Value2, typeof(OpenApiInteger))]
        [InlineData("integer", "int32", 10, typeof(OpenApiInteger))]
        [InlineData("integer", "int32", IntEnum.Value2, typeof(OpenApiInteger))]
        [InlineData("integer", "int64", 4294967295L, typeof(OpenApiLong))]
        [InlineData("number", "float", 1.2F, typeof(OpenApiFloat))]
        [InlineData("number", "double", 1.25D, typeof(OpenApiDouble))]
        [InlineData("string", "uuid", "d3966535-2637-48fa-b911-e3c27405ee09", typeof(OpenApiString))]
        [InlineData("string", null, "foobar", typeof(OpenApiString))]
        public void CreateFor_CreatesAnInstance_ForProvidedSchemaAndValue(
            string schemaType,
            string schemaFormat,
            object value,
            Type expectedInstanceType)
        {
            var schema = new OpenApiSchema { Type = schemaType, Format = schemaFormat };

            var instance = OpenApiAnyFactory.CreateFor(schema, value);

            Assert.NotNull(instance);
            Assert.IsType(expectedInstanceType, instance);
        }

        [Fact]
        public void CreateFor_CreatesAnInstance_ForDateTimeSchemaAndValue()
        {
            var schema = new OpenApiSchema { Type = "string", Format = "date-time" };

            var instance = OpenApiAnyFactory.CreateFor(schema, DateTime.UtcNow);

            Assert.NotNull(instance);
            Assert.IsType<OpenApiDate>(instance);
        }

        [Fact]
        public void CreateFor_CreatesAnInstance_ForDoubleSchemaAndValueWhenGivenDecimal()
        {
            var schema = new OpenApiSchema { Type = "number", Format = "double" };

            var instance = OpenApiAnyFactory.CreateFor(schema, 3.4m);

            Assert.NotNull(instance);
            Assert.IsType<OpenApiDouble>(instance);
        }

        [Fact]
        public void CreateFor_ReturnsNull_IfValueIsNull()
        {
            var schema = new OpenApiSchema { Type = "string" };

            var instance = OpenApiAnyFactory.CreateFor(schema, null);

            Assert.Null(instance);
        }
    }
}
