using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.Swagger
{
    public class SwaggerOptions
    {
        internal const string DefaultRouteTemplate = "/swagger/{documentName}/swagger.{extension:regex(^(json|ya?ml)$)}";

        public SwaggerOptions()
        {
            PreSerializeFilters = new List<Action<OpenApiDocument, HttpRequest>>();
            SerializeAsV2 = false;
        }

        /// <summary>
        /// Sets a custom route for the Swagger JSON/YAML endpoint(s). Must include the {documentName} parameter
        /// </summary>
        public string RouteTemplate { get; set; } = DefaultRouteTemplate;

        /// <summary>
        /// Return Swagger JSON/YAML in the V2 format rather than V3
        /// </summary>
        public bool SerializeAsV2 { get; set; }

        /// <summary>
        /// Gets or sets an optional custom <see cref="ISwaggerDocumentSerializer"/> implementation to use to serialize Swagger documents.
        /// </summary>
        /// <remarks>For the CLI tool to be able to use this, this needs to be configured for use in the service collection of your application.</remarks>
        public ISwaggerDocumentSerializer CustomDocumentSerializer { get; set; }

        /// <summary>
        /// Actions that can be applied to an OpenApiDocument before it's serialized.
        /// Useful for setting metadata that's derived from the current request.
        /// </summary>
        public List<Action<OpenApiDocument, HttpRequest>> PreSerializeFilters { get; private set; }
    }
}
