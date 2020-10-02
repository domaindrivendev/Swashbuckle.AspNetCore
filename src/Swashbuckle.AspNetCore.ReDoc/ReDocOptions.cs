using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json.Serialization;

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
        public string SpecUrl { get; set; } = null;

        public ConfigObject ConfigObject { get; set; } = new ConfigObject();
    }

    public class ConfigObject
    {
        /// <summary>
        /// If set, the spec is considered untrusted and all HTML/markdown is sanitized to prevent XSS.
        /// Disabled by default for performance reasons. Enable this option if you work with untrusted user data!
        /// </summary>
        public bool UntrustedSpec { get; set; } = false;

        /// <summary>
        /// If set, specifies a vertical scroll-offset in pixels.
        /// This is often useful when there are fixed positioned elements at the top of the page, such as navbars, headers etc
        /// </summary>
        public int? ScrollYOffset { get; set; }

        /// <summary>
        /// If set, the protocol and hostname is not shown in the operation definition
        /// </summary>
        public bool HideHostname { get; set; } = false;

        /// <summary>
        /// Do not show "Download" spec button. THIS DOESN'T MAKE YOUR SPEC PRIVATE, it just hides the button
        /// </summary>
        public bool HideDownloadButton { get; set; } = false;

        /// <summary>
        /// Specify which responses to expand by default by response codes.
        /// Values should be passed as comma-separated list without spaces e.g. "200,201". Special value "all" expands all responses by default.
        /// Be careful: this option can slow-down documentation rendering time.
        /// </summary>
        public string ExpandResponses { get; set; } = "all";

        /// <summary>
        /// Show required properties first ordered in the same order as in required array
        /// </summary>
        public bool RequiredPropsFirst { get; set; } = false;

        /// <summary>
        /// Do not inject Authentication section automatically
        /// </summary>
        public bool NoAutoAuth { get; set; } = false;

        /// <summary>
        /// Show path link and HTTP verb in the middle panel instead of the right one
        /// </summary>
        public bool PathInMiddlePanel { get; set; } = false;

        /// <summary>
        /// Do not show loading animation. Useful for small docs
        /// </summary>
        public bool HideLoading { get; set; } = false;

        /// <summary>
        /// Use native scrollbar for sidemenu instead of perfect-scroll (scrolling performance optimization for big specs)
        /// </summary>
        public bool NativeScrollbars { get; set; } = false;

        /// <summary>
        /// Disable search indexing and search box
        /// </summary>
        public bool DisableSearch { get; set; } = false;

        /// <summary>
        /// Show only required fields in request samples
        /// </summary>
        public bool OnlyRequiredInSamples { get; set; } = false;

        /// <summary>
        /// Sort properties alphabetically
        /// </summary>
        public bool SortPropsAlphabetically { get; set; } = false;

        [JsonExtensionData]
        public Dictionary<string, object> AdditionalItems { get; set; } = new Dictionary<string, object>();
    }
}