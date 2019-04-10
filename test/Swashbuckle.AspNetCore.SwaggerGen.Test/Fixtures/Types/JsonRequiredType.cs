using Newtonsoft.Json;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    [JsonObject(ItemRequired = Required.Always)]
    public class JsonRequiredType
    {
        [JsonProperty(Required = Required.AllowNull)]
        public string ValAllowNull { get; set; }

        [JsonProperty(Required = Required.Default)]
        public string ValDefault { get; set; }

        // Required = Required.Always from JsonObjectAttribute
        [JsonProperty]
        public string ValWithoutReq { get; set; }

        // Required = Required.Always from JsonObjectAttribute
        public string ValueWithoutAttr { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string ValAlways { get; set; }

        [JsonProperty(Required = Required.DisallowNull)]
        public string ValDisallowNull { get; set; }
    }
}