using System;
using System.Collections.Generic;
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
        /// Provide a custom ISwaggerDocumentSerializer implementation, to have more control over how the swagger document is exactly serialized.
        /// </summary>
        /// <remarks>For the CLI tool to be able to use this, this needs to be set during ConfigureServices.</remarks>
        public ISwaggerDocumentSerializer CustomDocumentSerializer { get; set; } = null;

        /// <summary>
        /// Actions that can be applied to an OpenApiDocument before it's serialized.
        /// Useful for setting metadata that's derived from the current request.
        /// </summary>
        public List<Action<OpenApiDocument, HttpRequest>> PreSerializeFilters { get; private set; }
    }
}
