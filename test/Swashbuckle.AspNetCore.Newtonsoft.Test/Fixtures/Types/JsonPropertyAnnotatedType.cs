using Newtonsoft.Json;

namespace Swashbuckle.AspNetCore.Newtonsoft.Test
{
    public class JsonPropertyAnnotatedType
    {
        [JsonProperty("string-with-json-property-name")]
        public string StringWithJsonPropertyName { get; set; }

        [JsonProperty(Required = Required.Default)]
        public int IntWithRequiredDefault { get; set; }

        [JsonProperty(Required = Required.Default)]
        public string StringWithRequiredDefault { get; set; }

        [JsonProperty(Required = Required.DisallowNull)]
        public string StringWithRequiredDisallowNull { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string StringWithRequiredAlways { get; set; }

        [JsonProperty(Required = Required.AllowNull)]
        public string StringWithRequiredAllowNull { get; set; }
    }
}