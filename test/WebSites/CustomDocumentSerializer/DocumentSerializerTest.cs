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

        var name = specVersion switch
        {
            OpenApiSpecVersion.OpenApi2_0 => "DocumentSerializerTest2.0",
            OpenApiSpecVersion.OpenApi3_0 => "DocumentSerializerTest3.0",
            _ => throw new NotImplementedException()
        };

        writer.WriteProperty(OpenApiConstants.Swagger, name);

        writer.WriteEndObject();
    }
}
