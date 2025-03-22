using System.Text.Json.Serialization;

namespace Swashbuckle.AspNetCore.SwaggerUI;

public class UrlDescriptor
{
    [JsonPropertyName("url")]
    public string Url { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }
}
