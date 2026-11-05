using System.Text;
using Swashbuckle.AspNetCore.ReDoc;

namespace Microsoft.AspNetCore.Builder;

public static class ReDocOptionsExtensions
{
    /// <summary>
    /// Injects additional CSS stylesheets into the index.html page
    /// </summary>
    /// <param name="options"></param>
    /// <param name="path">A path to the stylesheet - i.e. the link "href" attribute</param>
    /// <param name="media">The target media - i.e. the link "media" attribute</param>
    public static void InjectStylesheet(this ReDocOptions options, string path, string media = "screen")
    {
        var builder = new StringBuilder(options.HeadContent);
        builder.AppendLine($"<link href='{path}' rel='stylesheet' media='{media}' type='text/css' />");
        options.HeadContent = builder.ToString();
    }

    /// <summary>
    /// Sets the Swagger JSON endpoint. Can be fully-qualified or relative to the redoc page
    /// </summary>
    public static void SpecUrl(this ReDocOptions options, string url)
    {
        options.SpecUrl = url;
    }

    /// <summary>
    /// If enabled, the spec is considered untrusted and all HTML/markdown is sanitized to prevent XSS.
    /// Disabled by default for performance reasons. Enable this option if you work with untrusted user data!
    /// </summary>
    /// <param name="options"></param>
    public static void EnableUntrustedSpec(this ReDocOptions options)
    {
        options.ConfigObject.UntrustedSpec = true;
    }

    /// <summary>
    /// Specifies a vertical scroll-offset in pixels.
    /// This is often useful when there are fixed positioned elements at the top of the page, such as navbars, headers etc
    /// </summary>
    /// <param name="options"></param>
    /// <param name="offset"></param>
    public static void ScrollYOffset(this ReDocOptions options, int offset)
    {
        options.ConfigObject.ScrollYOffset = offset;
    }

    /// <summary>
    /// Do not show "Download" spec button. THIS DOESN'T MAKE YOUR SPEC PRIVATE, it just hides the button
    /// </summary>
    public static void HideDownloadButton(this ReDocOptions options)
    {
        options.ConfigObject.HideDownloadButton = true;
    }

    /// <summary>
    /// Show required properties first ordered in the same order as in required array
    /// </summary>
    public static void RequiredPropsFirst(this ReDocOptions options)
    {
        options.ConfigObject.RequiredPropsFirst = true;
    }

    /// <summary>
    /// Do not show loading animation. Useful for small docs
    /// </summary>
    public static void HideLoading(this ReDocOptions options)
    {
        options.ConfigObject.HideLoading = true;
    }

    /// <summary>
    /// Show only required fields in request samples
    /// </summary>
    public static void OnlyRequiredInSamples(this ReDocOptions options)
    {
        options.ConfigObject.OnlyRequiredInSamples = true;
    }
}
