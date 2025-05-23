using Newtonsoft.Json;

namespace Swashbuckle.AspNetCore.Newtonsoft.Test;

/// <summary>
/// For ad-hoc serializer testing
/// </summary>
public class NewtonsoftSerializerTesting
{
    [Fact]
    public void Serialize()
    {
        var dto = new TestKeyedCollection
        {
            new TestDto { Prop1 = "foo" },
            new TestDto { Prop1 = "bar" },
        };

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
