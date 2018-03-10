using System;
using System.IO;
using System.Reflection;

namespace Swashbuckle.AspNetCore.ReDoc
{
    public class ReDocOptions
    {
        /// <summary>
        /// Gets or sets a route prefix for accessing the redoc page
        /// </summary>
        public string RoutePrefix { get; set; } = "api-docs";

        /// <summary>
        /// Gets or sets a Stream function for retrieving the redoc page
        /// </summary>
        public Func<Stream> IndexStream { get; set; } = () => typeof(ReDocOptions).GetTypeInfo().Assembly
            .GetManifestResourceStream("Swashbuckle.AspNetCore.ReDoc.index.html");

        /// <summary>
        /// Gets or sets a title for the redoc page
        /// </summary>
        public string DocumentTitle { get; set; } = "API Docs";

        /// <summary>
        /// Gets or sets additional content to place in the head of the redoc page
        /// </summary>
        public string HeadContent { get; set; } = "";

        /// <summary>
        /// Gets or sets the Swagger JSON endpoint. Can be fully-qualified or relative to the redoc page
        /// </summary>
        public string SpecUrl { get; set; } = "v1/swagger.json";
    }
}