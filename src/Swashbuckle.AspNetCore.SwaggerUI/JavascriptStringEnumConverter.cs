#if NET6_0_OR_GREATER
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Swashbuckle.AspNetCore.SwaggerUI;

internal sealed class JavascriptStringEnumConverter<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] TEnum>() :
#if NET8_0_OR_GREATER
    JsonStringEnumConverter<TEnum>(JsonNamingPolicy.CamelCase, false)
#else
    JsonStringEnumConverter(JsonNamingPolicy.CamelCase, false)
#endif
    where TEnum : struct, Enum
{
}
#endif
