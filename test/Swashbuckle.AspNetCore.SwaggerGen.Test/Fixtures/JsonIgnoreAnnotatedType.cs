using System.Text.Json.Serialization;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    public class JsonIgnoreAnnotatedType
    {
        [JsonIgnore]
        public string StringWithJsonIgnore { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public string StringWithJsonIgnoreConditionNever { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public string StringWithJsonIgnoreConditionAlways { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string StringWithJsonIgnoreConditionWhenWritingDefault { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string StringWithJsonIgnoreConditionWhenWritingNull { get; set; }

        public string StringWithNoAnnotation { get; set; }
    }
}
