using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Swashbuckle.AspNetCore.TestSupport
{
    public class JsonSerializerAnnotatedType
    {
        public string StringWithNoAnnotation { get; set; }

        [JsonIgnore]
        public string StringWithJsonIgnore { get; set; }

        [JsonPropertyName("string-with-json-property-name")]
        public string StringWithJsonPropertyName { get; set; }

        [JsonExtensionData]
        public IDictionary<string, object> ExtensionData { get; set; }
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum JsonSerializerAnnotatedEnum
    {
        Value1 = 2,
        Value2 = 4,
        X = 8
    }
}
