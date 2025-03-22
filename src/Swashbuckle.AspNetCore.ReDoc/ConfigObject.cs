using System.Text.Json.Serialization;

namespace Swashbuckle.AspNetCore.ReDoc;

public class ConfigObject
{
    /// <summary>
    /// If set, the spec is considered untrusted and all HTML/markdown is sanitized to prevent XSS.
    /// Disabled by default for performance reasons. Enable this option if you work with untrusted user data!
    /// </summary>
    public bool UntrustedSpec { get; set; }

    /// <summary>
    /// If set, specifies a vertical scroll-offset in pixels.
    /// This is often useful when there are fixed positioned elements at the top of the page, such as navbars, headers etc
    /// </summary>
    public int? ScrollYOffset { get; set; }

    /// <summary>
    /// If set, the protocol and hostname is not shown in the operation definition
    /// </summary>
    public bool HideHostname { get; set; }

    /// <summary>
    /// Do not show "Download" spec button. THIS DOESN'T MAKE YOUR SPEC PRIVATE, it just hides the button
    /// </summary>
    public bool HideDownloadButton { get; set; }

    /// <summary>
    /// Specify which responses to expand by default by response codes.
    /// Values should be passed as comma-separated list without spaces e.g. "200,201". Special value "all" expands all responses by default.
    /// Be careful: this option can slow-down documentation rendering time.
    /// </summary>
    public string ExpandResponses { get; set; } = "all";

    /// <summary>
    /// Show required properties first ordered in the same order as in required array
    /// </summary>
    public bool RequiredPropsFirst { get; set; }

    /// <summary>
    /// Do not inject Authentication section automatically
    /// </summary>
    public bool NoAutoAuth { get; set; }

    /// <summary>
    /// Show path link and HTTP verb in the middle panel instead of the right one
    /// </summary>
    public bool PathInMiddlePanel { get; set; }

    /// <summary>
    /// Do not show loading animation. Useful for small docs
    /// </summary>
    public bool HideLoading { get; set; }

    /// <summary>
    /// Use native scrollbar for sidemenu instead of perfect-scroll (scrolling performance optimization for big specs)
    /// </summary>
    public bool NativeScrollbars { get; set; }

    /// <summary>
    /// Disable search indexing and search box
    /// </summary>
    public bool DisableSearch { get; set; }

    /// <summary>
    /// Show only required fields in request samples
    /// </summary>
    public bool OnlyRequiredInSamples { get; set; }

    /// <summary>
    /// Sort properties alphabetically
    /// </summary>
    public bool SortPropsAlphabetically { get; set; }

    [JsonExtensionData]
    public Dictionary<string, object> AdditionalItems { get; set; } = [];
}
