using System;
using System.Collections.Generic;
using System.Text.Json;
using Xunit;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    /// <summary>
    /// For ad-hoc serializer testing
    /// </summary>
    public class JsonSerializerTesting
    {
        [Fact]
        public void Serialize()
        {
            var dto = new Dictionary<JsonConverterAnnotatedEnum, string>
            {
                [JsonConverterAnnotatedEnum.Value1] = "foo",
                [JsonConverterAnnotatedEnum.Value2] = "bar",
                [JsonConverterAnnotatedEnum.X] = "blah",
            };

            Assert.Throws<NotSupportedException>(() =>
            {
                JsonSerializer.Serialize(dto);
            });
        }

        [Fact]
        public void Deserialize()
        {
            Assert.Throws<NotSupportedException>(() =>
            {
                var dto = JsonSerializer.Deserialize<Dictionary<JsonConverterAnnotatedEnum, string>>(
                    "{ \"value1\": \"foo\", \"value2\": \"bar\" }");
            });
        }
    }
}
