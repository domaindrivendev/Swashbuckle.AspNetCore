using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Swashbuckle.AspNetCore.SwaggerUI;

internal sealed class JavascriptStringEnumConverter<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] TEnum>() :
    JsonStringEnumConverter<TEnum>(JsonNamingPolicy.CamelCase, false)
    where TEnum : struct, Enum;
