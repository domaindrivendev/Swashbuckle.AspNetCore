using System;
using System.IO;
using System.Reflection;
using Newtonsoft.Json.Linq;

namespace Swashbuckle.AspNetCore.SwaggerUI
{
    public class SwaggerUIOptions
    {
        /// <summary>
        /// Gets or sets a route prefix for accessing the swagger-ui
        /// </summary>
        public string RoutePrefix { get; set; } = "swagger";

        /// <summary>
        /// Gets or sets a Stream function for retrieving the swagger-ui page
        /// </summary>
        public Func<Stream> IndexStream { get; set; } = () => typeof(SwaggerUIOptions).GetTypeInfo().Assembly
            .GetManifestResourceStream("Swashbuckle.AspNetCore.SwaggerUI.index.html");

        /// <summary>
        /// Gets or sets a title for the swagger-ui page
        /// </summary>
        public string DocumentTitle { get; set; } = "Swagger UI";

        /// <summary>
        /// Gets or sets additional content to place in the head of the swagger-ui page
        /// </summary>
        public string HeadContent { get; set; } = "";

        /// <summary>
        /// Gets the JavaScript config object, represented as JSON, that will be passed to the SwaggerUI
        /// </summary>
        public JObject ConfigObject { get; } = JObject.FromObject(new
        {
            urls = new object[] { }
        });
    }
}