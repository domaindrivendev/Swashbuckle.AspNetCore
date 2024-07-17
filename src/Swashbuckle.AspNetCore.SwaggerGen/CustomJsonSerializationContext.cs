#if NET8_0_OR_GREATER

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Swashbuckle.AspNetCore.SwaggerGen;

[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(JsonElement))]
internal sealed partial class CustomJsonSerializerContext : JsonSerializerContext;

#endif
