using System.Text.Json.Nodes;

namespace Swashbuckle.AspNetCore;

internal static class JsonModelFactory
{
    public static JsonNode CreateFromJson(string json)
        => json is null ? null : JsonNode.Parse(json);
}
