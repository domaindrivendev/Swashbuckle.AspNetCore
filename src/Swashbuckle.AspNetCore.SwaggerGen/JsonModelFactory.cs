using System.Text.Json.Nodes;
using Microsoft.OpenApi;

namespace Swashbuckle.AspNetCore;

#nullable enable

internal static class JsonModelFactory
{
    public static JsonNode? CreateFromJson(string? json)
    {
        if (json is null)
            return null;

        if (json == "null")
            return JsonNullSentinel.JsonNull;

        return JsonNode.Parse(json);
    }
}
