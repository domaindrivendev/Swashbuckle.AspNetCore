#if NET
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Swashbuckle.AspNetCore.ReDoc;

[JsonSerializable(typeof(ConfigObject))]
// These primitive types are declared for common types that may be used with ConfigObject.AdditionalItems. See https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/2884.
[JsonSerializable(typeof(bool))]
[JsonSerializable(typeof(byte))]
[JsonSerializable(typeof(sbyte))]
[JsonSerializable(typeof(short))]
[JsonSerializable(typeof(ushort))]
[JsonSerializable(typeof(int))]
[JsonSerializable(typeof(int?))]
[JsonSerializable(typeof(uint))]
[JsonSerializable(typeof(long))]
[JsonSerializable(typeof(ulong))]
[JsonSerializable(typeof(float))]
[JsonSerializable(typeof(double))]
[JsonSerializable(typeof(decimal))]
[JsonSerializable(typeof(char))]
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(DateTime))]
[JsonSerializable(typeof(DateTimeOffset))]
[JsonSerializable(typeof(TimeSpan))]
[JsonSerializable(typeof(JsonArray))]
[JsonSerializable(typeof(JsonObject))]
[JsonSerializable(typeof(JsonDocument))]
[JsonSerializable(typeof(DateOnly))]
[JsonSerializable(typeof(TimeOnly))]
[JsonSerializable(typeof(Half))]
[JsonSerializable(typeof(Int128))]
[JsonSerializable(typeof(UInt128))]
[JsonSourceGenerationOptions(
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal sealed partial class ReDocOptionsJsonContext : JsonSerializerContext;
#endif
