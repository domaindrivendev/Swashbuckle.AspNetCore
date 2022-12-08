using System.Text;
using Swashbuckle.AspNetCore.Redoc;

namespace Microsoft.AspNetCore.Builder
{
    public static class RedocOptionsExtensions
    {
        /// <summary>
        /// Injects additional CSS stylesheets into the index.html page
        /// </summary>
        /// <param name="options"></param>
        /// <param name="path">A path to the stylesheet - i.e. the link "href" attribute</param>
        /// <param name="media">The target media - i.e. the link "media" attribute</param>
        public static void InjectStylesheet(this RedocOptions options, string path, string media = "screen")
        {
            var builder = new StringBuilder(options.HeadContent);
            builder.AppendLine($"<link href='{path}' rel='stylesheet' media='{media}' type='text/css' />");
            options.HeadContent = builder.ToString();
        }

        /// <summary>
        /// Sets the Swagger JSON endpoint. Can be fully-qualified or relative to the redoc page
        /// </summary>
        public static void SpecUrl(this RedocOptions options, string url)
        {
            options.SpecUrl = url;
        }

        /// <summary>
        /// If enabled, the spec is considered untrusted and all HTML/markdown is sanitized to prevent XSS.
        /// Disabled by default for performance reasons. Enable this option if you work with untrusted user data!
        /// </summary>
        /// <param name="options"></param>
        public static void EnableUntrustedSpec(this RedocOptions options)
        {
            options.ConfigObject.UntrustedSpec = true;
        }

        /// <summary>
        /// Specifies a vertical scroll-offset in pixels.
        /// This is often useful when there are fixed positioned elements at the top of the page, such as navbars, headers etc
        /// </summary>
        /// <param name="options"></param>
        /// <param name="offset"></param>
        public static void ScrollYOffset(this RedocOptions options, int offset)
        {
            options.ConfigObject.ScrollYOffset = offset;
        }

        /// <summary>
        /// Controls if the protocol and hostname is shown in the operation definition
        /// </summary>
        public static void HideHostname(this RedocOptions options)
        {
            options.ConfigObject.HideHostname = true;
        }

        /// <summary>
        /// Do not show "Download" spec button. THIS DOESN'T MAKE YOUR SPEC PRIVATE, it just hides the button
        /// </summary>
        public static void HideDownloadButton(this RedocOptions options)
        {
            options.ConfigObject.HideDownloadButton = true;
        }

        /// <summary>
        /// Specify which responses to expand by default by response codes.
        /// Values should be passed as comma-separated list without spaces e.g. "200,201". Special value "all" expands all responses by default.
        /// Be careful: this option can slow-down documentation rendering time.
        /// Default is "all"
        /// </summary>
        public static void ExpandResponses(this RedocOptions options, string responses)
        {
            options.ConfigObject.ExpandResponses = responses;
        }

        /// <summary>
        /// Show required properties first ordered in the same order as in required array
        /// </summary>
        public static void RequiredPropsFirst(this RedocOptions options)
        {
            options.ConfigObject.RequiredPropsFirst = true;
        }

        /// <summary>
        /// Do not inject Authentication section automatically
        /// </summary>
        public static void NoAutoAuth(this RedocOptions options)
        {
            options.ConfigObject.NoAutoAuth = true;
        }

        /// <summary>
        /// Show path link and HTTP verb in the middle panel instead of the right one
        /// </summary>
        public static void PathInMiddlePanel(this RedocOptions options)
        {
            options.ConfigObject.PathInMiddlePanel = true;
        }

        /// <summary>
        /// Do not show loading animation. Useful for small docs
        /// </summary>
        public static void HideLoading(this RedocOptions options)
        {
            options.ConfigObject.HideLoading = true;
        }

        /// <summary>
        /// Use native scrollbar for sidemenu instead of perfect-scroll (scrolling performance optimization for big specs)
        /// </summary>
        public static void NativeScrollbars(this RedocOptions options)
        {
            options.ConfigObject.NativeScrollbars = true;
        }

        /// <summary>
        /// Disable search indexing and search box
        /// </summary>
        public static void DisableSearch(this RedocOptions options)
        {
            options.ConfigObject.DisableSearch = true;
        }

        /// <summary>
        /// Show only required fields in request samples
        /// </summary>
        public static void OnlyRequiredInSamples(this RedocOptions options)
        {
            options.ConfigObject.OnlyRequiredInSamples = true;
        }

        /// <summary>
        /// Sort properties alphabetically
        /// </summary>
        public static void SortPropsAlphabetically(this RedocOptions options)
        {
            options.ConfigObject.SortPropsAlphabetically = true;
        }
    }
}
