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
            BaseType dto = new SubType1 { BaseProperty = "foo", Property1 = 123 };

            var jsonString = JsonConvert.SerializeObject(
                dto,
                new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All,
                    TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Full
                });

            //throw new Exception(jsonString);
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
