using System.Text.Json.Serialization;

namespace Swashbuckle.AspNetCore.ReDoc;

public class ConfigObject
{
    /// <summary>
    /// If set, the spec is considered untrusted and all HTML/markdown is sanitized to prevent XSS.
    /// Disabled by default for performance reasons. Enable this option if you work with untrusted user data!
    /// </summary>
    /// <remarks>
    /// This option is serialized as <c>sanitize</c>, which is the name used by Redoc 3.
    /// </remarks>
    [JsonPropertyName("sanitize")]
    public bool UntrustedSpec { get; set; }

    /// <summary>
    /// If set, specifies a vertical scroll-offset in pixels.
    /// This is often useful when there are fixed positioned elements at the top of the page, such as navbars, headers etc
    /// </summary>
    public int? ScrollYOffset { get; set; }

    /// <summary>
    /// Do not show "Download" spec button. THIS DOESN'T MAKE YOUR SPEC PRIVATE, it just hides the button
    /// </summary>
    /// <remarks>
    /// This option is serialized as <c>hideDownloadButtons</c>, which is the name used by Redoc 3.
    /// </remarks>
    [JsonPropertyName("hideDownloadButtons")]
    public bool HideDownloadButton { get; set; }

    /// <summary>
    /// Show required properties first ordered in the same order as in required array
    /// </summary>
    /// <remarks>
    /// This option is serialized as <c>sortRequiredPropsFirst</c>, which is the name used by Redoc 3.
    /// </remarks>
    [JsonPropertyName("sortRequiredPropsFirst")]
    public bool RequiredPropsFirst { get; set; }

    /// <summary>
    /// Do not show loading animation. Useful for small docs
    /// </summary>
    public bool HideLoading { get; set; }

    /// <summary>
    /// Show only required fields in request samples
    /// </summary>
    public bool OnlyRequiredInSamples { get; set; }

    [JsonExtensionData]
    public Dictionary<string, object> AdditionalItems { get; set; } = [];
}
