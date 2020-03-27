using System.Text.Json.Serialization;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    public class JsonIgnoreAnnotatedType
    {
        [JsonIgnore]
        public string StringWithJsonIgnore { get; set; }

        public string StringWithNoAnnotation { get; set; }
    }
}
