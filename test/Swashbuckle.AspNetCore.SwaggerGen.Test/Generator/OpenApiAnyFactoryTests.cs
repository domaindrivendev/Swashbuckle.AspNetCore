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

            // now check in the case where the schema is an array type (has Items)
            var arraySchema = new OpenApiSchema { Items = new OpenApiSchema { Type = schemaType, Format = schemaFormat } };

            OpenApiAnyFactory.TryCreateFor(arraySchema, value, out IOpenApiAny arrayInstance);

            Assert.NotNull(arrayInstance);
            Assert.IsType(typeof(OpenApiArray), arrayInstance);
            Assert.IsType(expectedInstanceType, ((OpenApiArray)arrayInstance)[0]);
        }

        [Theory]
        [InlineData("boolean", null, typeof(OpenApiBoolean), true, false, true)]
        [InlineData("integer", "int32", typeof(OpenApiInteger), (byte)10, (byte)11)]
        [InlineData("integer", "int32", typeof(OpenApiInteger), ByteEnum.Value2, ByteEnum.Value2)]
        [InlineData("integer", "int32", typeof(OpenApiInteger), (short)10, (short)11)]
        [InlineData("integer", "int32", typeof(OpenApiInteger), ShortEnum.Value2, ShortEnum.Value2)]
        [InlineData("integer", "int32", typeof(OpenApiInteger), 10, 11, 12)]
        [InlineData("integer", "int32", typeof(OpenApiInteger), IntEnum.Value2, IntEnum.Value2)]
        [InlineData("integer", "int64", typeof(OpenApiLong), 4294967295L, 4294967296L)]
        [InlineData("number", "float", typeof(OpenApiFloat), 1.2F, 1.3F)]
        [InlineData("number", "double", typeof(OpenApiDouble), 1.25D, 1.50D)]
        [InlineData("string", "uuid", typeof(OpenApiString), "d3966535-2637-48fa-b911-e3c27405ee09", "d3966535-2637-48fa-b911-e3c27405ee0a")]
        [InlineData("string", null, typeof(OpenApiString), "foobar", "baz")]
        public void TryCreateFor_CreatesAnInstance_ForProvidedArraySchemaAndValue(
            string schemaType,
            string schemaFormat,
            Type expectedInstanceType,
            params object[] values)
        {
            var schema = new OpenApiSchema { Items = new OpenApiSchema { Type = schemaType, Format = schemaFormat } };

            OpenApiAnyFactory.TryCreateFor(schema, values, out IOpenApiAny instance);

            Assert.NotNull(instance);
            Assert.IsType(typeof(OpenApiArray), instance);
            Assert.Equal(values.Length, ((OpenApiArray)instance).Count);
            Assert.All((OpenApiArray)instance, v => v.GetType().Equals(expectedInstanceType));
        }

        [Fact]
        public void TryCreateFor_CreatesAnInstance_ForDateTimeSchemaAndValue()
        {
            var schema = new OpenApiSchema { Type = "string", Format = "date-time" };

            OpenApiAnyFactory.TryCreateFor(schema, DateTime.UtcNow, out IOpenApiAny instance);

            Assert.NotNull(instance);
            Assert.IsType(typeof(OpenApiDate), instance);
        }

        [Fact]
        public void TryCreateFor_CreatesAnInstance_ForDoubleSchemaAndValueWhenGivenDecimal()
        {
            var schema = new OpenApiSchema { Type = "number", Format = "double" };

            OpenApiAnyFactory.TryCreateFor(schema, 3.4m, out IOpenApiAny instance);

            Assert.NotNull(instance);
            Assert.IsType(typeof(OpenApiDouble), instance);
        }
    }
}
