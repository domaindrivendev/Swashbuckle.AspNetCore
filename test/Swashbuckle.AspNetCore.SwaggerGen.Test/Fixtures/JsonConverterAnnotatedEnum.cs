using System.Text.Json.Serialization;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum JsonConverterAnnotatedEnum
    {
        Value1 = 2,
        Value2 = 4,
        X = 8
    }
}
