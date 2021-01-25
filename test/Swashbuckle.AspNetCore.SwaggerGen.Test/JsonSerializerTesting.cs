using System.Collections.ObjectModel;
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
            var dto = new TestKeyedCollection();
            dto.Add(new TestDto { Prop1 = "foo" });
            dto.Add(new TestDto { Prop1 = "bar" });

            var json = JsonSerializer.Serialize(dto);

            Assert.Equal("[{\"Prop1\":\"foo\"},{\"Prop1\":\"bar\"}]", json);
        }

        [Fact]
        public void Deserialize()
        {
            var dto = JsonSerializer.Deserialize<TestDto>(
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
