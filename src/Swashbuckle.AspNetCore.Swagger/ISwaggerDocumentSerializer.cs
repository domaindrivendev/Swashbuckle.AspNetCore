using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Writers;

namespace Swashbuckle.AspNetCore.Swagger
{
    /// <summary>
    /// Provide an implementation for this interface if you wish to customize how the open api document is exactly written.
    /// SerializeDocument will be called in that case, instead of the Microsoft built in Serialize methods.
    /// </summary>
    public interface ISwaggerDocumentSerializer
    {
        /// <summary>
        /// Called in the places where normally SerializeV2 or SerializeV3 on OpenApiDocument would be called.
        /// </summary>
        /// <param name="document">The open api document that should be serialized</param>
        /// <param name="writer">The write to which the document needs to be written</param>
        /// <param name="specVersion">The open api spec to serialize</param>
		void SerializeDocument(OpenApiDocument document, IOpenApiWriter writer, OpenApiSpecVersion specVersion);
	}
}
