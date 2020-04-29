using System.Collections.Generic;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.TestSupport;
using Xunit;

namespace Swashbuckle.AspNetCore.Newtonsoft.Test
{
    /// <summary>
    /// For ad-hoc serializer testing
    /// </summary>
    public class NewtonsoftSerializerTesting
    {
        [Fact]
        public void Serialize()
        {
            var dto = new Dictionary<IntEnum, string>
            {
                [IntEnum.Value2] = "foo",
                [IntEnum.Value4] = "bar",
                [IntEnum.Value8] = "blah",
            };

            var jsonString = JsonConvert.SerializeObject(dto);
            Assert.Equal("{\"Value2\":\"foo\",\"Value4\":\"bar\",\"Value8\":\"blah\"}", jsonString);
        }

        [Fact]
        public void Deserialize()
        {
            var dto = JsonConvert.DeserializeObject<TestDto>(
                "{ \"jsonRequired\": \"foo\", \"jsonProperty\": null }");

            Assert.Equal("foo", dto.jsonRequired);
            Assert.Null(dto.jsonProperty);
        }
    }

    public class TestDto
    {
        [JsonRequired]
        public string jsonRequired;

        [JsonProperty(Required = Required.AllowNull)]
        public string jsonProperty;
    }
}
