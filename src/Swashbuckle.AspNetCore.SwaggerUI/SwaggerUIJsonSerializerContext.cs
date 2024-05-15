#if NET6_0_OR_GREATER
using System.Text.Json.Serialization;

namespace Swashbuckle.AspNetCore.SwaggerUI;

[JsonSerializable(typeof(ConfigObject))]
[JsonSerializable(typeof(InterceptorFunctions))]
[JsonSerializable(typeof(OAuthConfigObject))]
[JsonSourceGenerationOptions(
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal sealed partial class SwaggerUIOptionsJsonContext : JsonSerializerContext;
#endif
