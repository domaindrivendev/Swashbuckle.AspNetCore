using System.Collections.ObjectModel;
using Newtonsoft.Json;
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
            var dto = new TestKeyedCollection();
            dto.Add(new TestDto { Prop1 = "foo" });
            dto.Add(new TestDto { Prop1 = "bar" });

            var json = JsonConvert.SerializeObject(dto);

            Assert.Equal("[{\"Prop1\":\"foo\"},{\"Prop1\":\"bar\"}]", json);
        }

        [Fact]
        public void Deserialize()
        {
            var dto = JsonConvert.DeserializeObject<TestDto>(
                "{ \"Prop1\": \"foo\" }");
        }
    }

    public class TestDto
    {
        public string Prop1 { get; set; }
    }

    public class TestKeyedCollection : KeyedCollection<string, TestDto>
    {
        protected override string GetKeyForItem(TestDto item)
        {
            return item.Prop1;
        }
    }
}
