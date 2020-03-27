using Newtonsoft.Json;
using Xunit;
using JsonSerializer = System.Text.Json.JsonSerializer;
using Swashbuckle.AspNetCore.TestSupport;

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
            var dto = JsonSerializer.Deserialize<TypeWithOverriddenProperty>("{ \"Property1\": \"foobar\" }");

            Assert.Equal("foobar", dto.Property1);
        }

        //[Fact]
        public void Newtonsoft()
        {
            var dto = JsonConvert.DeserializeObject<TypeWithOverriddenProperty>("{ \"Property1\": \"foobar\" }");

            Assert.Equal("foobar", dto.Property1);
        } 
    }
}
