using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.Extensions.Options;

namespace Swashbuckle.AspNetCore.Swagger
{
    public class SwaggerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly SwaggerOptions _options;
        private readonly TemplateMatcher _requestMatcher;
        private readonly SwaggerDocumentBuilder _swaggerDocumentBuilder;

        public SwaggerMiddleware(
            RequestDelegate next,
            IOptions<SwaggerOptions> optionsAccessor)
            : this(next, optionsAccessor.Value)
        { }

        public SwaggerMiddleware(
            RequestDelegate next,
            SwaggerOptions options)
        {
            _next = next;
            _options = options ?? new SwaggerOptions();
            _requestMatcher = new TemplateMatcher(TemplateParser.Parse(_options.RouteTemplate), new RouteValueDictionary());
            _swaggerDocumentBuilder = new SwaggerDocumentBuilder(_options);
        }

        public async Task Invoke(HttpContext httpContext, ISwaggerProvider swaggerProvider)
        {
            if (!RequestingSwaggerDocument(httpContext.Request, out string documentName))
            {
                await _next(httpContext);
                return;
            }

            try
            {
                var swaggerDocument =
                    _swaggerDocumentBuilder.Build(
                        httpContext.Request,
                        swaggerProvider,
                        documentName);

                await RespondWithSwaggerJson(httpContext.Response, swaggerDocument);
            }
            catch (UnknownSwaggerDocument)
            {
                RespondWithNotFound(httpContext.Response);
            }
        }

        private bool RequestingSwaggerDocument(HttpRequest request, out string documentName)
        {
            documentName = null;
            if (request.Method != "GET") return false;

            var routeValues = new RouteValueDictionary();
            if (!_requestMatcher.TryMatch(request.Path, routeValues) || !routeValues.ContainsKey("documentName"))
            {
                return false;
            }

            documentName = routeValues["documentName"].ToString();
            return true;
        }

        private void RespondWithNotFound(HttpResponse response)
        {
            response.StatusCode = 404;
        }

        private async Task RespondWithSwaggerJson(HttpResponse response, string swaggerDocument)
        {
            response.StatusCode = 200;
            response.ContentType = "application/json;charset=utf-8";

            await response.WriteAsync(swaggerDocument, new UTF8Encoding(false));
        }
    }
}
