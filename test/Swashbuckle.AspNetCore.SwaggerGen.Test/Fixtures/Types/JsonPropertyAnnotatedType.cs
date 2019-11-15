
using System.Text.Json.Serialization;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    public class JsonPropertyAnnotatedType
    {
        [JsonIgnore]
        public string StringWithJsonIgnore { get; set; }

        [JsonPropertyName("string-with-json-property-name")]
        public string StringWithJsonPropertyName { get; set; }

        public int IntProperty { get; set; }

        public int? NullableIntProperty { get; set; }
    }
}