#nullable enable

using Microsoft.AspNetCore.Http;

namespace Swashbuckle.AspNetCore;

internal static class HttpContextAcceptEncodingCheckExtensions
{
    public static bool IsGZipAccepted(this HttpContext httpContext)
    {
        var acceptEncoding = httpContext.Request.Headers.AcceptEncoding;

        for (var index = 0; index < acceptEncoding.Count; index++)
        {
            var stringValue = acceptEncoding[index].AsSpan();
            if (stringValue.Contains("gzip", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
