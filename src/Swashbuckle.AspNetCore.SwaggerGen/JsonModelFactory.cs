#if NET10_0_OR_GREATER
using System.Text.Json.Nodes;
#else
using Microsoft.OpenApi.Any;
using Swashbuckle.AspNetCore.SwaggerGen;
#endif

namespace Swashbuckle.AspNetCore;

internal static class JsonModelFactory
{
#if NET10_0_OR_GREATER
    public static JsonNode CreateFromJson(string json)
        => json is null ? null : JsonNode.Parse(json);
#else
    public static IOpenApiAny CreateFromJson(string json)
        => OpenApiAnyFactory.CreateFromJson(json);
#endif
}
