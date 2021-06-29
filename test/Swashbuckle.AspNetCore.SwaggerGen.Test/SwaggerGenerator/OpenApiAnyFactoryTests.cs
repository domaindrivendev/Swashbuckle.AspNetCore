namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    using System;
    using System.Text.Json;
    using Microsoft.OpenApi.Any;
    using Xunit;

    public class OpenApiAnyFactoryTests
    {
        [Theory]
        [InlineData("1", typeof(OpenApiInteger), 1)]
        [InlineData("4294877294", typeof(OpenApiLong), 4294877294L)]
        [InlineData("1.5", typeof(OpenApiFloat), 1.5f)]
        [InlineData("1.5e308", typeof(OpenApiDouble), 1.5e308)]
        [InlineData("\"abc\"", typeof(OpenApiString), "abc")]
        [InlineData("true", typeof(OpenApiBoolean), true)]
        [InlineData("false", typeof(OpenApiBoolean), false)]
        public void CreateFromJson_SimpleType(string json, Type expectedType, object expectedValue)
        {
            var openApiAnyObject = OpenApiAnyFactory.CreateFromJson(json);
            Assert.NotNull(openApiAnyObject);
            Assert.Equal(expectedType, openApiAnyObject.GetType());
            Assert.Equal(AnyType.Primitive, openApiAnyObject.AnyType);
            var valueProperty = expectedType.GetProperty("Value");
            var actualValue = valueProperty.GetValue(openApiAnyObject);
            Assert.Equal(expectedValue, actualValue);
        }

        [Fact]
        public void CreateFromJson_NullType()
        {
            var expectedType = typeof(OpenApiNull);

            var openApiAnyObject = OpenApiAnyFactory.CreateFromJson("null");
            Assert.NotNull(openApiAnyObject);
            Assert.Equal(expectedType, openApiAnyObject.GetType());
            Assert.Equal(AnyType.Null, openApiAnyObject.AnyType);
            var valueProperty = expectedType.GetProperty("Value");
            Assert.Null(valueProperty);
        }

        [Theory]
        [InlineData("[1,2]", typeof(OpenApiInteger), 1, 2)]
        [InlineData("[4294877294,4294877295]", typeof(OpenApiLong), 4294877294L, 4294877295L)]
        [InlineData("[1.5,-1.5]", typeof(OpenApiFloat), 1.5f, -1.5f)]
        [InlineData("[1.5e308,-1.5e308]", typeof(OpenApiDouble), 1.5e308, -1.5e308)]
        [InlineData("[\"abc\",\"def\"]", typeof(OpenApiString), "abc", "def")]
        [InlineData("[true,false]", typeof(OpenApiBoolean), true, false)]
        [InlineData("[{\"a\":1,\"b\":2},{\"a\":3,\"b\":4}]", typeof(OpenApiObject))]
        [InlineData("[[1,2],[3,4]]", typeof(OpenApiArray))]
        public void CreateFromJson_Array(string json, Type expectedType, params object[] expectedValues)
        {
            var openApiAnyObject = OpenApiAnyFactory.CreateFromJson(json);
            Assert.NotNull(openApiAnyObject);
            Assert.Equal(typeof(OpenApiArray), openApiAnyObject.GetType());
            Assert.Equal(AnyType.Array, openApiAnyObject.AnyType);
            var array = (OpenApiArray)openApiAnyObject;
            for (var i = 0; i < array.Count; i++)
            {
                Assert.NotNull(array[i]);
                Assert.Equal(expectedType, array[i].GetType());
                if (expectedValues.Length > 0)
                {
                    var valueProperty = expectedType.GetProperty("Value");
                    var expectedValue = expectedValues[i];
                    var actualValue = valueProperty.GetValue(array[i]);
                    Assert.Equal(expectedValue, actualValue);
                }
            }
        }

        [Fact]
        public void CreateFromJson_Object()
        {
            var json = JsonSerializer.Serialize(new
            {
                int_value = 1,
                long_value = 4294877294L,
                float_value = 1.5f,
                double_value = 1.5e308,
                string_value = "abc",
                true_value = true,
                false_value = false,
                array_value = new[] {1,2},
                object_value = new
                {
                    a = 1,
                    b = 2
                }
            });

            var openApiAnyObject = OpenApiAnyFactory.CreateFromJson(json);
            Assert.NotNull(openApiAnyObject);
            Assert.Equal(typeof(OpenApiObject), openApiAnyObject.GetType());
            Assert.Equal(AnyType.Object, openApiAnyObject.AnyType);
            var obj = (OpenApiObject)openApiAnyObject;

            Assert.NotNull(obj["int_value"]);
            Assert.Equal(typeof(OpenApiInteger), obj["int_value"].GetType());
            Assert.Equal(AnyType.Primitive, obj["int_value"].AnyType);
            Assert.Equal(1, ((OpenApiInteger) obj["int_value"]).Value);

            Assert.NotNull(obj["long_value"]);
            Assert.Equal(typeof(OpenApiLong), obj["long_value"].GetType());
            Assert.Equal(AnyType.Primitive, obj["long_value"].AnyType);
            Assert.Equal(4294877294L, ((OpenApiLong)obj["long_value"]).Value);

            Assert.NotNull(obj["float_value"]);
            Assert.Equal(typeof(OpenApiFloat), obj["float_value"].GetType());
            Assert.Equal(AnyType.Primitive, obj["float_value"].AnyType);
            Assert.Equal(1.5f, ((OpenApiFloat)obj["float_value"]).Value);

            Assert.NotNull(obj["double_value"]);
            Assert.Equal(typeof(OpenApiDouble), obj["double_value"].GetType());
            Assert.Equal(AnyType.Primitive, obj["double_value"].AnyType);
            Assert.Equal(1.5e308, ((OpenApiDouble)obj["double_value"]).Value);

            Assert.NotNull(obj["string_value"]);
            Assert.Equal(typeof(OpenApiString), obj["string_value"].GetType());
            Assert.Equal(AnyType.Primitive, obj["string_value"].AnyType);
            Assert.Equal("abc", ((OpenApiString)obj["string_value"]).Value);

            Assert.NotNull(obj["true_value"]);
            Assert.Equal(typeof(OpenApiBoolean), obj["true_value"].GetType());
            Assert.Equal(AnyType.Primitive, obj["true_value"].AnyType);
            Assert.True(((OpenApiBoolean)obj["true_value"]).Value);

            Assert.NotNull(obj["false_value"]);
            Assert.Equal(typeof(OpenApiBoolean), obj["false_value"].GetType());
            Assert.Equal(AnyType.Primitive, obj["false_value"].AnyType);
            Assert.False(((OpenApiBoolean)obj["false_value"]).Value);

            Assert.NotNull(obj["array_value"]);
            Assert.Equal(typeof(OpenApiArray), obj["array_value"].GetType());
            Assert.Equal(AnyType.Array, obj["array_value"].AnyType);

            Assert.NotNull(obj["object_value"]);
            Assert.Equal(typeof(OpenApiObject), obj["object_value"].GetType());
            Assert.Equal(AnyType.Object, obj["object_value"].AnyType);
        }
    }
}