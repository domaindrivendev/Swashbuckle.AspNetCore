using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Template;

namespace Swashbuckle.AspNetCore.SwaggerUi
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
            _requestMatcher = new TemplateMatcher(TemplateParser.Parse(options.RoutePrefix), new RouteValueDictionary());
            _options = options;
            _resourceAssembly = GetType().GetTypeInfo().Assembly;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            var request = httpContext.Request;

            if (!RequestingSwaggerUi(request))
            {
                await _next(httpContext);
                return;
            }

            // Enforce trailing slash so that relative paths are resolved correctly
            if (!request.Path.Value.EndsWith("/"))
            {
                httpContext.Response.Redirect(request.Path + "/");
                return;
            }

            var template = _resourceAssembly.GetManifestResourceStream("Swashbuckle.AspNetCore.SwaggerUi.CustomAssets.index.html");
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
