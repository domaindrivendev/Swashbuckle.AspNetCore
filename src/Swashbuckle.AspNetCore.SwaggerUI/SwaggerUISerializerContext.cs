#if NET6_0_OR_GREATER
using System.Text.Json.Serialization;

namespace Swashbuckle.AspNetCore.SwaggerUI
{
    [JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Default)]
    [JsonSerializable(typeof(ConfigObject))]
    [JsonSerializable(typeof(OAuthConfigObject))]
    [JsonSerializable(typeof(InterceptorFunctions))]
    internal sealed partial class SwaggerUISerializerContext : JsonSerializerContext
    {
    }
}
#endif
