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
            var json = JsonSerializer.Serialize(float.MaxValue);

            Assert.Equal("3.4028235E+38", json);
        }

        [Fact]
        public void Deserialize()
        {
            var dto = JsonSerializer.Deserialize<TestDto>(
                "{ \"Prop1\": 123 }");
        }
    }

    public class TestDto
    {
        public int Prop1 { get; set; }
    }
}
