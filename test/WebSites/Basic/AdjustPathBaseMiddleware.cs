using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Basic
{
    /// <summary>
    /// Required for testing swagger behind a <see cref="HttpRequest.PathBase"/>.
    /// This middleware allows to set the PathBase through a custom header for easier testing.
    /// </summary>
    public class AdjustPathBaseMiddleware
    {
        private readonly RequestDelegate _next;

        public AdjustPathBaseMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            string pathBase = GetPathBaseFromHeader(context);
            if (pathBase != null)
            {
                context.Request.PathBase = pathBase + context.Request.PathBase;
            }

            await _next(context);
        }

        private string GetPathBaseFromHeader(HttpContext context)
        {
            StringValues pathBaseObj;
            if (context.Request.Headers.TryGetValue("X-Forwarded-PathBase", out pathBaseObj))
            {
                string pathBase = pathBaseObj.ToString();

                // Security Checks
                if (!pathBase.StartsWith("/"))
                    return null;

                return pathBase;
            }

            return null;
        }
    }
}