using System.Threading.Tasks;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Swashbuckle.AspNetCore.SwaggerUI
{
    public class SwaggerUIIndexMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly SwaggerUIOptions _options;
        private string _basePath;

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

            _basePath = new Uri($"{httpContext.Request.Scheme}://{httpContext.Request.Host}{httpContext.Request.Path}/").ToString();

            await RespondWithIndexHtml(httpContext.Response);
            return;
        }

        public bool RequestingSwaggerUIIndex(HttpRequest request)
        {
            return (request.Method == "GET"
                && Regex.IsMatch(request.Path, $"^/{_options.RoutePrefix}/?$"));
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
                { "%(OAuthConfigObject)", SerializeToJson(_options.OAuthConfigObject) },
                { "%(SwaggerUIBasePath)", _basePath }
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
