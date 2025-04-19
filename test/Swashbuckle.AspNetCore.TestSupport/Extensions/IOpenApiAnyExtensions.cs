using Microsoft.OpenApi;
using Microsoft.OpenApi.Writers;

namespace Swashbuckle.AspNetCore.TestSupport;

public static class IOpenApiAnyExtensions
{
    public static string ToJson(this Microsoft.OpenApi.Any.OpenApiAny openApiAny)
    {
        var stringWriter = new StringWriter();
        var jsonWriter = new OpenApiJsonWriter(stringWriter);

        // Use 3.0 for consistency with previous versions of Microsoft.OpenApi
        openApiAny.Write(jsonWriter, OpenApiSpecVersion.OpenApi3_0);

        return stringWriter.ToString();
    }
}
