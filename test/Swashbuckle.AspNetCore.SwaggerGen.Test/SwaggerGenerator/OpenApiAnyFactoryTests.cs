using System.Text.Json;
using Xunit;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    public class OpenApiAnyFactoryTests
    {
        [Fact]
        public void CreateFromJson_Int32()
        {
            var openApiAnyObject = OpenApiAnyFactory.CreateFromJson("1");
            Assert.NotNull(openApiAnyObject);
            Assert.Equal(JsonValueKind.Number, openApiAnyObject.GetValueKind());
            Assert.Equal(1, openApiAnyObject.GetValue<int>());
        }

        [Fact]
        public void CreateFromJson_Int64()
        {
            var openApiAnyObject = OpenApiAnyFactory.CreateFromJson("4294877294");
            Assert.NotNull(openApiAnyObject);
            Assert.Equal(JsonValueKind.Number, openApiAnyObject.GetValueKind());
            Assert.Equal(4294877294L, openApiAnyObject.GetValue<long>());
        }

        [Fact]
        public void CreateFromJson_Float()
        {
            var openApiAnyObject = OpenApiAnyFactory.CreateFromJson("1.5");
            Assert.NotNull(openApiAnyObject);
            Assert.Equal(JsonValueKind.Number, openApiAnyObject.GetValueKind());
            Assert.Equal(1.5f, openApiAnyObject.GetValue<float>());
        }

        [Fact]
        public void CreateFromJson_Double()
        {
            var openApiAnyObject = OpenApiAnyFactory.CreateFromJson("1.5e308");
            Assert.NotNull(openApiAnyObject);
            Assert.Equal(JsonValueKind.Number, openApiAnyObject.GetValueKind());
            Assert.Equal(1.5e308, openApiAnyObject.GetValue<double>());
        }

        [Fact]
        public void CreateFromJson_String()
        {
            var openApiAnyObject = OpenApiAnyFactory.CreateFromJson("\"abc\"");
            Assert.NotNull(openApiAnyObject);
            Assert.Equal(JsonValueKind.String, openApiAnyObject.GetValueKind());
            Assert.Equal("abc", openApiAnyObject.GetValue<string>());
        }

        [Theory]
        [InlineData("true", JsonValueKind.True, true)]
        [InlineData("false", JsonValueKind.False, false)]
        public void CreateFromJson_Boolean(string json, JsonValueKind expectedKind, object expectedValue)
        {
            var openApiAnyObject = OpenApiAnyFactory.CreateFromJson(json);
            Assert.NotNull(openApiAnyObject);
            Assert.Equal(expectedKind, openApiAnyObject.GetValueKind());
            Assert.Equal(expectedValue, openApiAnyObject.GetValue<bool>());
        }

        [Fact]
        public void CreateFromJson_NullType()
        {
            var openApiAnyObject = OpenApiAnyFactory.CreateFromJson("null");
            Assert.Null(openApiAnyObject);
        }

        [Fact]
        public void CreateFromJson_Array_Int32()
        {
            CreateFromJsonArray("[1,2]", JsonValueKind.Number, [1, 2]);
        }

        [Fact]
        public void CreateFromJson_Array_Int64()
        {
            CreateFromJsonArray("[4294877294,4294877295]", JsonValueKind.Number, [4294877294L, 4294877295L]);
        }

        [Fact]
        public void CreateFromJson_Array_Float()
        {
            CreateFromJsonArray("[1.5,-1.5]", JsonValueKind.Number, [1.5f, -1.5f]);
        }

        [Fact]
        public void CreateFromJson_Array_Double()
        {
            CreateFromJsonArray("[1.5e308,-1.5e308]", JsonValueKind.Number, [1.5e308, -1.5e308]);
        }

        [Fact]
        public void CreateFromJson_Array_String()
        {
            CreateFromJsonArray("[\"abc\",\"def\"]", JsonValueKind.String, ["abc", "def"]);
        }

        [Fact]
        public void CreateFromJson_Array_Boolean()
        {
            CreateFromJsonArray("[true,true]", JsonValueKind.True, [true, true]);
        }

        [Fact]
        public void CreateFromJson_Array_Object()
        {
            CreateFromJsonArray<object>("[{\"a\":1,\"b\":2},{\"a\":3,\"b\":4}]", JsonValueKind.Object, []);
        }

        [Fact]
        public void CreateFromJson_Array_Array()
        {
            CreateFromJsonArray<int[]>("[[1,2],[3,4]]", JsonValueKind.Array, []);
        }

        private static void CreateFromJsonArray<T>(string json, JsonValueKind expectedType, params T[] expectedValues)
        {
            var openApiAnyObject = OpenApiAnyFactory.CreateFromJson(json);
            Assert.NotNull(openApiAnyObject);
            Assert.Equal(JsonValueKind.Array, openApiAnyObject.GetValueKind());
            var array = openApiAnyObject.AsArray();
            for (var i = 0; i < array.Count; i++)
            {
                Assert.NotNull(array[i]);
                Assert.Equal(expectedType, array[i].GetValueKind());
                if (expectedValues.Length > 0)
                {
                    var expectedValue = expectedValues[i];
                    var actualValue = array[i].GetValue<T>();
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
                array_value = new int[] { 1, 2 },
                object_value = new
                {
                    a = 1,
                    b = 2
                }
            });

            var openApiAnyObject = OpenApiAnyFactory.CreateFromJson(json);
            Assert.NotNull(openApiAnyObject);
            Assert.Equal(JsonValueKind.Object, openApiAnyObject.GetValueKind());
            var obj = openApiAnyObject.AsObject();

            Assert.NotNull(obj["int_value"]);
            Assert.Equal(JsonValueKind.Number, obj["int_value"].GetValueKind());
            Assert.Equal(1, obj["int_value"].GetValue<int>());

            Assert.NotNull(obj["long_value"]);
            Assert.Equal(JsonValueKind.Number, obj["long_value"].GetValueKind());
            Assert.Equal(4294877294L, obj["long_value"].GetValue<long>());

            Assert.NotNull(obj["float_value"]);
            Assert.Equal(JsonValueKind.Number, obj["float_value"].GetValueKind());
            Assert.Equal(1.5f, obj["float_value"].GetValue<float>());

            Assert.NotNull(obj["double_value"]);
            Assert.Equal(JsonValueKind.Number, obj["double_value"].GetValueKind());
            Assert.Equal(1.5e308, obj["double_value"].GetValue<double>());

            Assert.NotNull(obj["string_value"]);
            Assert.Equal(JsonValueKind.String, obj["string_value"].GetValueKind());
            Assert.Equal("abc", obj["string_value"].GetValue<string>());

            Assert.NotNull(obj["true_value"]);
            Assert.Equal(JsonValueKind.True, obj["true_value"].GetValueKind());
            Assert.True(obj["true_value"].GetValue<bool>());

            Assert.NotNull(obj["false_value"]);
            Assert.Equal(JsonValueKind.False, obj["false_value"].GetValueKind());
            Assert.False(obj["false_value"].GetValue<bool>());

            Assert.NotNull(obj["array_value"]);
            Assert.Equal(JsonValueKind.Array, obj["array_value"].GetValueKind());
            Assert.Equal([1, 2], obj["array_value"].AsArray().GetValues<int>());

            Assert.NotNull(obj["object_value"]);
            Assert.Equal(JsonValueKind.Object, obj["object_value"].GetValueKind());
        }
    }
}
