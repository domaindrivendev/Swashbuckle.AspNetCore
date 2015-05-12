using System;
using System.Globalization;
using Microsoft.AspNet.Http;

namespace Swashbuckle.Application
{
    public class DefaultRootUrlResolver : IRootUrlResolver
    {
        public string ResolveFrom(HttpRequest request)
        {
            var requestUri = new Uri(string.Format("{0}://{1}{2}", request.Scheme, request.Host, request.PathBase));

            var scheme = request.Headers["X-Forwarded-Proto"] ?? requestUri.Scheme;
            var host = request.Headers["X-Forwarded-Host"] ?? requestUri.Host;
            var port = request.Headers["X-Forwarded-Port"] ?? requestUri.Port.ToString(CultureInfo.InvariantCulture);

            return string.Format("{0}://{1}:{2}{3}", scheme, host, port, request.PathBase);
        }
    }
}
