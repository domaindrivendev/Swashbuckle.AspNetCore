using Newtonsoft.Json;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    [JsonObject(ItemRequired = Required.Always)]
    public class JsonRequiredNullableType
    {
        [JsonProperty(Required = Required.AllowNull)]
        public int? ValAllowNull { get; set; }

        [JsonProperty(Required = Required.Default)]
        public int? ValDefault { get; set; }

        // Required = Required.Always from JsonObjectAttribute
        [JsonProperty]
        public int? ValWithoutReq { get; set; }

        // Required = Required.Always from JsonObjectAttribute
        public int? ValueWithoutAttr { get; set; }

        [JsonProperty(Required = Required.Always)]
        public int? ValAlways { get; set; }

        [JsonProperty(Required = Required.DisallowNull)]
        public int? ValDisallowNull { get; set; }
    }
}