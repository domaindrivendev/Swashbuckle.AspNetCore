using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Swashbuckle.Test.Fixtures
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum JsonConvertedEnum
    {
        Value1 = 2,
        Value2 = 4
    }
}