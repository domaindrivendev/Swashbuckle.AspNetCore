using Newtonsoft.Json;
using Xunit;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    /// <summary>
    /// For ad-hoc serializer testing
    /// </summary>
    public class SerializerTesting
    {
        //[Fact]
        public void SystemTextJson()
        {
            var dto = JsonSerializer.Deserialize<TestDto>("{ \"Prop1\": \"foobar\" }");

            Assert.Equal("foobar", dto.Prop1);
        }

        //[Fact]
        public void Newtonsoft()
        {
            var dto = JsonConvert.DeserializeObject<TestDto>("{ \"Prop1\": \"foobar\" }");

            Assert.Equal("foobar", dto.Prop1);
        } 
    }

    public class TestDto
    {
        public TestDto(string prop1)
        {
            Prop1 = prop1;
        }

        public string Prop1 { get; }
    }
}
