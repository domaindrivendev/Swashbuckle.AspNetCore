using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Swashbuckle.AspNetCore.Newtonsoft.Test
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum JsonConverterAnnotatedEnum
    {
        Value1 = 2,
        Value2 = 4,
        [EnumMember(Value = "X-foo")]
        X = 8
    }
}