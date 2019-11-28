using Newtonsoft.Json;

namespace Swashbuckle.AspNetCore.Newtonsoft.Test
{
    public class JsonIgnoreAnnotatedType
    {
        [JsonIgnore]
        public string StringWithJsonIgnore { get; set; }

        public string StringWithNoAnnotation { get; set; }
    }
}