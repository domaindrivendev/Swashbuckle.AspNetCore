using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.OpenApi;

namespace Swashbuckle.AspNetCore;

internal static class JsonExtensions
{
    private static readonly JsonSerializerOptions Options = new()
    {
#if NET9_0_OR_GREATER
        NewLine = "\n",
#endif
        WriteIndented = true,
    };

    public static string ToJson(this JsonNode value)
    {
        if (value == JsonNullSentinel.JsonNull)
        {
            return "null";
        }
        var json = value.ToJsonString(Options);

#if !NET9_0_OR_GREATER
        json = json.Replace("\r\n", "\n");
#endif

        return json;
    }
}
