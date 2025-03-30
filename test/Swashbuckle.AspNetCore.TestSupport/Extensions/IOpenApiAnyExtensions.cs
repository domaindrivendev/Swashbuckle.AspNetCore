using Microsoft.OpenApi;
using Microsoft.OpenApi.Writers;

#if NET10_0_OR_GREATER
using Any = Microsoft.OpenApi.Any.OpenApiAny;
#else
using Any = Microsoft.OpenApi.Any.IOpenApiAny;
#endif

namespace Swashbuckle.AspNetCore.TestSupport;

public static class IOpenApiAnyExtensions
{
    public static string ToJson(this Any openApiAny)
    {
        var stringWriter = new StringWriter();
        var jsonWriter = new OpenApiJsonWriter(stringWriter);

        // Use 3.0 for consistency with previous versions of Microsoft.OpenApi
        openApiAny.Write(jsonWriter, OpenApiSpecVersion.OpenApi3_0);

        return stringWriter.ToString();
    }
}
