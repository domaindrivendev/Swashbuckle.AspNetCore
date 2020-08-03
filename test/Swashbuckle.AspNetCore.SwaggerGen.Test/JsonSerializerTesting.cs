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
            var dto = new TestDto();

            var json = JsonSerializer.Serialize(dto);

            //Assert.Equal("{\"Prop1\":null}", json);
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
