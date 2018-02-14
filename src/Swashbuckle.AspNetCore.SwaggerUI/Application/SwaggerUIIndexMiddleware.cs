using System.Threading.Tasks;
using System.IO;
using System.Text;
using System.Reflection;
using Microsoft.AspNetCore.Http;

namespace Swashbuckle.AspNetCore.SwaggerUI
{
    public class SwaggerUIIndexMiddleware
    {
        private const string EmbeddedIndexName = "Swashbuckle.AspNetCore.SwaggerUI.Template.index.html";

        private readonly RequestDelegate _next;
        private readonly SwaggerUIOptions _options;

        public SwaggerUIIndexMiddleware(RequestDelegate next, SwaggerUIOptions options)
        {
            _next = next;
            _options = options;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if (!RequestingSwaggerUIIndex(httpContext.Request))
            {
                await _next(httpContext);
                return;
            }

            RespondWithIndexHtml(httpContext.Response);
        }

        private bool RequestingSwaggerUIIndex(HttpRequest request)
        {
            var indexPath = string.IsNullOrEmpty(_options.RoutePrefix) ? "/" : $"/{_options.RoutePrefix}/";
            return (request.Method == "GET" && request.Path == indexPath);
        }

        private async void RespondWithIndexHtml(HttpResponse response)
        {
            response.StatusCode = 200;
            response.ContentType = "text/html";

            using (var rawStream = _options.IndexStream())
            {
                var rawText = new StreamReader(rawStream).ReadToEnd();
                var htmlBuilder = new StringBuilder(rawText);
                foreach (var entry in _options.IndexSettings.ToTemplateParameters())
                {
                    htmlBuilder.Replace(entry.Key, entry.Value);
                }

                await response.WriteAsync(htmlBuilder.ToString(), Encoding.UTF8);
            }
        }
    }
}
