using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Writers;
using Swashbuckle.AspNetCore.Swagger;

namespace CustomDocumentSerializer;

public class DocumentSerializerTest : ISwaggerDocumentSerializer
{
    public void SerializeDocument(OpenApiDocument document, IOpenApiWriter writer, OpenApiSpecVersion specVersion)
    {
        writer.WriteStartObject();
        switch (specVersion)
        {
            case OpenApiSpecVersion.OpenApi2_0:
                writer.WriteProperty(OpenApiConstants.Swagger, "DocumentSerializerTest2.0");
                break;
            case OpenApiSpecVersion.OpenApi3_0:
                writer.WriteProperty(OpenApiConstants.Swagger, "DocumentSerializerTest3.0");
                break;
#if NET10_0_OR_GREATER
            case OpenApiSpecVersion.OpenApi3_1:
                writer.WriteProperty(OpenApiConstants.Swagger, "DocumentSerializerTest3.1");
                break;
#endif
            default:
                throw new NotImplementedException();
        }
        writer.WriteEndObject();
    }
}
