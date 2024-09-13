using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;

namespace Swashbuckle.AspNetCore.Scalar
{
    public class ScalarOptions
    {
        /// <summary>
        /// Gets or sets a route prefix for accessing the Scalar page
        /// </summary>
        public string RoutePrefix { get; set; } = "api-reference";

        /// <summary>
        /// Gets or sets a Stream function for retrieving the Scalar page
        /// </summary>
        public Func<Stream> IndexStream { get; set; } = () => ResourceHelper.GetEmbeddedResource("index.html");

        /// <summary>
        /// Gets or sets a title for the Scalar page
        /// </summary>
        public string DocumentTitle { get; set; } = "API Docs";

        /// <summary>
        /// Gets or sets additional content to place in the head of the Scalar page
        /// </summary>
        public string HeadContent { get; set; } = "";

        /// <summary>
        /// Gets or sets the Swagger JSON endpoint. Can be fully-qualified or relative to the Scalar page
        /// </summary>
        public string SpecUrl { get; set; } = null;

        public ConfigObject Config { get; set; } = new ConfigObject();
    }

    public class ConfigObject
    {
        /// <summary>
        /// Whether the Swagger editor should be shown.
        /// </summary>
        public bool? IsEditable { get; set; } = null;
        /// <summary>
        /// Select the theme.
        /// Can be one of: alternate, default, moon, purple, solarized, bluePlanet, saturn, kepler, mars, deepSpace, none
        /// </summary>
        public string Theme { get; set; } = null;

        /// <summary>
        /// Additional configuration data, documentation can be found at
        /// https://github.com/scalar/scalar/blob/main/documentation/configuration.md.
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, object> AdditionalItems { get; set; } = new Dictionary<string, object>();
    }
}
