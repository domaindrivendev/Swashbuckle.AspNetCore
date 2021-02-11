using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.Swagger
{
    public class SwaggerEndpointOptions
    {
        public SwaggerEndpointOptions()
        {
            PreSerializeFilters = new List<Action<OpenApiDocument, HttpRequest>>();
            SerializeAsV2 = false;
            UseOpenApiDocumentMemoryCaching = false;
        }


        /// <summary>
        /// Return Swagger JSON in the V2 format rather than V3
        /// </summary>
        public bool SerializeAsV2 { get; set; }

        /// <summary>
        /// Allows to cache generated OpenApiDocument for identical requests until
        /// applying <see cref="PreSerializeFilters"/>.
        /// </summary>
        public bool UseOpenApiDocumentMemoryCaching { get; set; }

        /// <summary>
        /// Actions that can be applied SwaggerDocument's before they're serialized to JSON.
        /// Useful for setting metadata that's derived from the current request
        /// </summary>
        public List<Action<OpenApiDocument, HttpRequest>> PreSerializeFilters { get; private set; }
    }
}
