using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.OpenApi;

namespace Swashbuckle.AspNetCore;

internal static class JsonExtensions
{
    private static readonly JsonSerializerOptions Options = new()
    {
        NewLine = "\n",
        WriteIndented = true,
    };

    public static string ToJson(this JsonNode value)
        => value.IsJsonNullSentinel() ? "null" : value.ToJsonString(Options);
}
