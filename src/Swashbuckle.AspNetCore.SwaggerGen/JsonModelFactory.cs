using Microsoft.OpenApi.Any;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Swashbuckle.AspNetCore;

internal static class JsonModelFactory
{
    public static IOpenApiAny CreateFromJson(string json)
        => OpenApiAnyFactory.CreateFromJson(json);
}
