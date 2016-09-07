using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Template;

namespace Swashbuckle.SwaggerUi.Application
{
    public class SwaggerUiMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly TemplateMatcher _requestMatcher;
        private readonly SwaggerUiOptions _options;
        private readonly Assembly _resourceAssembly;

        public SwaggerUiMiddleware(
            RequestDelegate next,
            SwaggerUiOptions options
        )
        {
            _next = next;
            _requestMatcher = new TemplateMatcher(TemplateParser.Parse(options.IndexPath), new RouteValueDictionary());
            _options = options;
            _resourceAssembly = GetType().GetTypeInfo().Assembly;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if (!RequestingSwaggerUi(httpContext.Request))
            {
                await _next(httpContext);
                return;
            }

            var template = _resourceAssembly.GetManifestResourceStream("Swashbuckle.SwaggerUi.CustomAssets.index.html");
            var content = GenerateContent(template, _options.IndexConfig.ToParamDictionary());

            RespondWithHtmlContent(httpContext.Response, content);
        }

        private bool RequestingSwaggerUi(HttpRequest request)
        {
            return (request.Method == "GET") && _requestMatcher.TryMatch(request.Path, new RouteValueDictionary());
        }
        
        private Stream GenerateContent(Stream template, IDictionary<string, string> templateParams)
        {
            var templateText = new StreamReader(template).ReadToEnd();
            var contentBuilder = new StringBuilder(templateText);
            foreach (var entry in templateParams)
            {
                contentBuilder.Replace(entry.Key, entry.Value);
            }

            return new MemoryStream(Encoding.UTF8.GetBytes(contentBuilder.ToString()));
        }

        private void RespondWithHtmlContent(HttpResponse response, Stream content)
        {
            response.StatusCode = 200;
            response.ContentType = "text/html";
            content.CopyTo(response.Body);
        }
    }
}
