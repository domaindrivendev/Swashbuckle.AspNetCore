using System.Threading.Tasks;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Swashbuckle.AspNetCore.SwaggerUI
{
    public class SwaggerUIIndexMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly SwaggerUIOptions _options;

        public SwaggerUIIndexMiddleware(RequestDelegate next, SwaggerUIOptions options)
        {
            _next = next;
            _options = options;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            var request = httpContext.Request;

            if (!RequestingSwaggerUIIndex(request))
            {
                await _next(httpContext);
                return;
            }

            // If trailing slash is missing, force via redirect
            if (!request.Path.Value.EndsWith("/"))
            {
                // NOTE: the redirect is relative to the last segment of the incoming request so that
                // the slash is added to the URL as seen by the client. This supports proxy-based setups
                RespondWithRedirect(httpContext.Response, $"{request.Path.Value.Split('/').Last()}/");
                return;
            }

            await RespondWithIndexHtml(httpContext.Response);
            return;
        }

        public bool RequestingSwaggerUIIndex(HttpRequest request)
        {
            return (request.Method == "GET"
                && Regex.IsMatch(request.Path, $"^/{_options.RoutePrefix}/?$"));
        }

        private void RespondWithRedirect(HttpResponse response, string redirectPath)
        {
            response.StatusCode = 301;
            response.Headers["Location"] = redirectPath;
        }

        private async Task RespondWithIndexHtml(HttpResponse response)
        {
            response.StatusCode = 200;
            response.ContentType = "text/html";

            using (var stream = _options.IndexStream())
            {
                // Inject parameters before writing to response
                var htmlBuilder = new StringBuilder(new StreamReader(stream).ReadToEnd());
                foreach (var entry in GetIndexParameters())
                {
                    htmlBuilder.Replace(entry.Key, entry.Value);
                }

                await response.WriteAsync(htmlBuilder.ToString(), Encoding.UTF8);
            }
        }

        private IDictionary<string, string> GetIndexParameters()
        {
            return new Dictionary<string, string>()
            {
                { "%(DocumentTitle)", _options.DocumentTitle },
                { "%(HeadContent)", _options.HeadContent },
                { "%(ConfigObject)", SerializeToJson(_options.ConfigObject) },
                { "%(OAuthConfigObject)", SerializeToJson(_options.OAuthConfigObject) }
            };
        }

        private string SerializeToJson(JObject jObject)
        {
            return JsonConvert.SerializeObject(jObject, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Include,
                Formatting = Formatting.None
            });
        }
    }
}
