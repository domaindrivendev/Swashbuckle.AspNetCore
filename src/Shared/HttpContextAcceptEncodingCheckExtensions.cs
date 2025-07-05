#nullable enable

using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Swashbuckle.AspNetCore;

/// <summary>
/// <see cref="HttpContext"/> Accept-Encoding check extensions
/// </summary>
internal static partial class HttpContextAcceptEncodingCheckExtensions
{
    private static readonly Regex s_gzipAcceptedCheckRegex = GetGZipAcceptedCheckRegex();

    [GeneratedRegex(@"(^|,)\s*gzip\s*(;|,|$)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex GetGZipAcceptedCheckRegex();

    /// <summary>
    /// Check is the <paramref name="httpContext"/> support gzip response
    /// </summary>
    /// <param name="httpContext"></param>
    /// <returns>is gzip response accepted</returns>
    public static bool IsGZipAccepted(this HttpContext httpContext)
    {
        return IsGZipAccepted(httpContext.Request.Headers.AcceptEncoding);
    }

    private static bool IsGZipAccepted(string? acceptEncoding) => !string.IsNullOrWhiteSpace(acceptEncoding) && s_gzipAcceptedCheckRegex.IsMatch(acceptEncoding);

    private static bool IsGZipAccepted(in StringValues acceptEncodingValues)
    {
        return acceptEncodingValues.Count switch
        {
            0 => false,
            1 => IsGZipAccepted(acceptEncodingValues[0]),
            _ => SlowCheckIsGZipAccepted(in acceptEncodingValues),
        };
    }

    private static bool SlowCheckIsGZipAccepted(in StringValues values)
    {
        var valuesCount = values.Count;
        for (var i = 0; i < valuesCount; i++)
        {
            if (IsGZipAccepted(values[i]))
            {
                return true;
            }
        }
        return false;
    }
}
