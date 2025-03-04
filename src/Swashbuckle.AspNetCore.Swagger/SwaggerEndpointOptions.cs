using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.Swagger
{
    public class SwaggerEndpointOptions
    {
        public SwaggerEndpointOptions()
        {
            PreSerializeFilters = [];
            OpenApiVersion = OpenApiSpecVersion.OpenApi3_0;
        }

        /// <summary>
        /// Return Swagger JSON/YAML in the V2.0 format rather than V3.0.
        /// </summary>
        [Obsolete($"This property will be removed in a future version of Swashbuckle.AspNetCore. Use the {nameof(OpenApiVersion)} property instead.")]
        public bool SerializeAsV2
        {
            get => OpenApiVersion == OpenApiSpecVersion.OpenApi2_0;
            set => OpenApiVersion = value ? OpenApiSpecVersion.OpenApi2_0 : OpenApiSpecVersion.OpenApi3_0;
        }

        /// <summary>
        /// Gets or sets the OpenAPI (Swagger) document version to use.
        /// </summary>
        /// <remarks>
        /// The default value is <see cref="OpenApiSpecVersion.OpenApi3_0"/>.
        /// </remarks>
        public OpenApiSpecVersion OpenApiVersion { get; set; }

        /// <summary>
        /// Actions that can be applied SwaggerDocument's before they're serialized to JSON.
        /// Useful for setting metadata that's derived from the current request
        /// </summary>
        public List<Action<OpenApiDocument, HttpRequest>> PreSerializeFilters { get; private set; }
    }
}
