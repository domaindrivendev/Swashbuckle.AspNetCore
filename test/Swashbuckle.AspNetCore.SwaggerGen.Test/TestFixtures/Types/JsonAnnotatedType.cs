using Newtonsoft.Json;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    public class JsonAnnotatedType
    {
        [JsonIgnore]
        public string StringWithJsonIgnore { get; set; }

        [JsonProperty("Foobar")]
        public string StringWithJsonPropertyName { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string StringWithJsonPropertyRequired { get; set; }
    }
}