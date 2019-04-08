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
        [InlineData("integer", "int32", (short)10, typeof(OpenApiInteger))]
        [InlineData("integer", "int32", ShortEnum.Value2, typeof(OpenApiInteger))]
        [InlineData("integer", "int32", 10, typeof(OpenApiInteger))]
        [InlineData("integer", "int32", IntEnum.Value2, typeof(OpenApiInteger))]
        [InlineData("integer", "int64", 4294967295L, typeof(OpenApiLong))]
        [InlineData("number", "float", 1.2F, typeof(OpenApiFloat))]
        [InlineData("number", "double", 1.25D, typeof(OpenApiDouble))]
        [InlineData("string", "uuid", "d3966535-2637-48fa-b911-e3c27405ee09", typeof(OpenApiString))]
        [InlineData("string", null, "foobar", typeof(OpenApiString))]
        public void TryCreateFor_CreatesAnInstance_ForProvidedSchemaAndValue(
            string schemaType,
            string schemaFormat,
            object value,
            Type expectedInstanceType)
        {
            var schema = new OpenApiSchema { Type = schemaType, Format = schemaFormat };

            OpenApiAnyFactory.TryCreateFor(schema, value, out IOpenApiAny instance);

            Assert.NotNull(instance);
            Assert.IsType(expectedInstanceType, instance);
        }

        [Fact]
        public void TryCreateFor_CreatesAnInstance_ForDateTimeSchemaAndValue()
        {
            var schema = new OpenApiSchema { Type = "string", Format = "date-time" };

            OpenApiAnyFactory.TryCreateFor(schema, DateTime.UtcNow, out IOpenApiAny instance);

            Assert.NotNull(instance);
            Assert.IsType(typeof(OpenApiDate), instance);
        }
    }
}
