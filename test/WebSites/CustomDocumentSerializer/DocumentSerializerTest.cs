using Microsoft.OpenApi;
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
            OpenApiSpecVersion.OpenApi3_1 => "DocumentSerializerTest3.1",
            _ => throw new NotImplementedException()
        };

        writer.WriteProperty(OpenApiConstants.Swagger, name);

        writer.WriteEndObject();
    }
}
