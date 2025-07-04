using Microsoft.OpenApi;

namespace Swashbuckle.AspNetCore.Swagger;

/// <summary>
/// Provide an implementation for this interface if you wish to customize how the OpenAPI document is written.
/// </summary>
public interface ISwaggerDocumentSerializer
{
    /// <summary>
    /// Serializes an OpenAPI document.
    /// </summary>
    /// <param name="document">The OpenAPI document that should be serialized.</param>
    /// <param name="writer">The writer to which the document needs to be written.</param>
    /// <param name="specVersion">The OpenAPI specification version to serialize as.</param>
    void SerializeDocument(OpenApiDocument document, IOpenApiWriter writer, OpenApiSpecVersion specVersion);
}
