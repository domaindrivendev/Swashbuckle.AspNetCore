using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace Swashbuckle.AspNetCore.Swagger
{
    public class SwaggerOptions
    {
        public SwaggerOptions()
        {
            PreSerializeFilters = new List<Action<SwaggerDocument, HttpRequest>>();
        }

        /// <summary>
        /// Sets a custom route for the Swagger JSON endpoint(s). Must include the {documentName} parameter
        /// </summary>
        public string RouteTemplate { get; set; } = "swagger/{documentName}/swagger.json";

        /// <summary>
        /// Actions that can be applied SwaggerDocument's before they're serialized to JSON.
        /// Useful for setting metadata that's derived from the current request
        /// </summary>
        public List<Action<SwaggerDocument, HttpRequest>> PreSerializeFilters { get; private set; }
    }
}
