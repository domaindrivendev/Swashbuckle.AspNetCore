using Newtonsoft.Json;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    public class JsonAnnotatedType
    {
        [JsonIgnore]
        public string StringWithJsonIgnore { get; set; }

        [JsonProperty("string-with-json-property-name")]
        public string StringWithJsonPropertyName { get; set; }

        [JsonProperty(Required = Required.AllowNull)]
        public string StringWithJsonPropertyRequiredAllowNull { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string StringWithJsonPropertyRequiredAlways { get; set; }

        [JsonProperty(Required = Required.DisallowNull)]
        public string StringWithJsonPropertyRequiredDisallowNull { get; set; }
    }
}