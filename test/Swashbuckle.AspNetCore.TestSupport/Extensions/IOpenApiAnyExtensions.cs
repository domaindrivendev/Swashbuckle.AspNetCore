using System.IO;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Writers;

using Any = Microsoft.OpenApi.Any.IOpenApiAny;

namespace Swashbuckle.AspNetCore.TestSupport
{
    public static class IOpenApiAnyExtensions
    {
        public static string ToJson(this Any openApiAny)
        {
            var stringWriter = new StringWriter();
            var jsonWriter = new OpenApiJsonWriter(stringWriter);

            openApiAny.Write(jsonWriter, OpenApiSpecVersion.OpenApi3_0);

            return stringWriter.ToString();
        }
    }
}
