using Newtonsoft.Json;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    [JsonObject(ItemRequired = Required.Always)]
    public class JsonObjectAnnotatedType
    {
        public string StringWithNoAnnotation { get; set; }

        [JsonProperty]
        public string StringWithRequiredUnspecified { get; set; }

        [JsonProperty(Required = Required.Default)]
        public string StringWithRequiredDefault { get; set; }

        [JsonProperty(Required = Required.DisallowNull)]
        public string StringWithRequiredDisallowNull { get; set; }

        [JsonProperty(Required = Required.AllowNull)]
        public string StringWithRequiredAllowNull { get; set; }
    }
}
