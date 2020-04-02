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
            var dto = JsonConvert.DeserializeObject<Dictionary<JsonConverterAnnotatedEnum, string>>(
                "{ \"value1\": \"foo\", \"value2\": \"bar\" }");

            Assert.Equal(new[] { JsonConverterAnnotatedEnum.Value1, JsonConverterAnnotatedEnum.Value2 }, dto.Keys);
        }
    }
}
