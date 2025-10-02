using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.Swagger;

namespace DocumentationSnippets;

public class CustomDocumentSerializer : ISwaggerDocumentSerializer
{
    public void SerializeDocument(OpenApiDocument document, IOpenApiWriter writer, OpenApiSpecVersion specVersion)
    {
        throw new NotImplementedException();
    }
}
