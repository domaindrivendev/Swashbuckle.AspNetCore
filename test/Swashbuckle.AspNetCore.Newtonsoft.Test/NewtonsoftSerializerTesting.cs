using System;
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
            var dto = new Version(1, 1, 1);

            var jsonString = JsonConvert.SerializeObject(dto);
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
