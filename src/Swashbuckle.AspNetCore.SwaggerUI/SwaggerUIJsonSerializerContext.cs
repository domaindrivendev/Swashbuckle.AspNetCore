#if NET6_0_OR_GREATER
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Swashbuckle.AspNetCore.SwaggerUI;

[JsonSerializable(typeof(ConfigObject))]
[JsonSerializable(typeof(InterceptorFunctions))]
[JsonSerializable(typeof(List<UrlDescriptor>))]
[JsonSerializable(typeof(OAuthConfigObject))]
// These primitive types are declared for common types that may be used with ConfigObject.AdditionalItems. See https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/2884.
[JsonSerializable(typeof(bool))]
[JsonSerializable(typeof(byte))]
[JsonSerializable(typeof(sbyte))]
[JsonSerializable(typeof(short))]
[JsonSerializable(typeof(ushort))]
[JsonSerializable(typeof(int))]
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
#if NET7_0_OR_GREATER
[JsonSerializable(typeof(DateOnly))]
[JsonSerializable(typeof(TimeOnly))]
#endif
#if NET8_0_OR_GREATER
[JsonSerializable(typeof(Half))]
[JsonSerializable(typeof(Int128))]
[JsonSerializable(typeof(UInt128))]
#endif
[JsonSourceGenerationOptions(
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal sealed partial class SwaggerUIOptionsJsonContext : JsonSerializerContext;
#endif
