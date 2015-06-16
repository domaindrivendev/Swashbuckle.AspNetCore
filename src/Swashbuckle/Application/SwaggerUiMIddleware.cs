using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Routing.Template;

namespace Swashbuckle.Application
{
    public class SwaggerUiMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly TemplateMatcher _requestMatcher;
        private readonly Assembly _resourceAssembly;
        private readonly string _swaggerUrl;

        public SwaggerUiMiddleware(
            RequestDelegate next,
            string routePrefix,
            string swaggerUrl)
        {
            _next = next;

            var indexPath = routePrefix.Trim('/') + "/index.html";
            _requestMatcher = new TemplateMatcher(TemplateParser.Parse(indexPath), null);
            _resourceAssembly = GetType().GetTypeInfo().Assembly;

            _swaggerUrl = swaggerUrl;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if (!RequestingSwaggerUi(httpContext.Request))
            {
                await _next(httpContext);
                return;
            }

            var template = _resourceAssembly.GetManifestResourceStream("Swashbuckle.SwaggerUi.index.html");
            var content = AssignPlaceholderValuesTo(template, httpContext);
            RespondWithContentHtml(httpContext.Response, content);
        }

        private bool RequestingSwaggerUi(HttpRequest request)
        {
            if (request.Method != "GET") return false;

            var routeValues = _requestMatcher.Match(request.Path.ToUriComponent().TrimStart('/'));
            if (routeValues == null) return false;

            return true;
        }

        private Stream AssignPlaceholderValuesTo(Stream template, HttpContext httpContext)
        {
            var swaggerUrl = httpContext.Request.PathBase + _swaggerUrl;
            var placeholderValues = new Dictionary<string, string>
            {
                { "%(SwaggerUrl)", swaggerUrl }
            };

            var templateText = new StreamReader(template).ReadToEnd();
            var contentBuilder = new StringBuilder(templateText);
            foreach (var entry in placeholderValues)
            {
                contentBuilder.Replace(entry.Key, entry.Value);
            }

            return new MemoryStream(Encoding.UTF8.GetBytes(contentBuilder.ToString()));
        }

        private void RespondWithContentHtml(HttpResponse response, Stream content)
        {
            response.StatusCode = 200;
            response.ContentType = "text/html";
            content.CopyTo(response.Body);
        }
    }
}
