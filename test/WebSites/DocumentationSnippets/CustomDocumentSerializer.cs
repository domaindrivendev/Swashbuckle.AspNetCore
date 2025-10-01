using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Writers;
using Swashbuckle.AspNetCore.Swagger;

namespace DocumentationSnippets;

public class CustomDocumentSerializer : ISwaggerDocumentSerializer
{
    public void SerializeDocument(OpenApiDocument document, IOpenApiWriter writer, OpenApiSpecVersion specVersion)
    {
        throw new NotImplementedException();
    }
}
