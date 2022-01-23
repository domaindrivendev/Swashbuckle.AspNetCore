using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Writers;

namespace Swashbuckle.AspNetCore.Swagger
{
    public class SwaggerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly SwaggerOptions _options;
        private readonly TemplateMatcher _requestMatcher;

        public SwaggerMiddleware(
            RequestDelegate next,
            SwaggerOptions options)
        {
            _next = next;
            _options = options ?? new SwaggerOptions();
            _requestMatcher = new TemplateMatcher(TemplateParser.Parse(_options.RouteTemplate), new RouteValueDictionary());
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
                var basePath = httpContext.Request.PathBase.HasValue
                    ? httpContext.Request.PathBase.Value
                    : null;

                var swagger = swaggerProvider.GetSwagger(
                    documentName: documentName,
                    host: null,
                    basePath: basePath);

                // One last opportunity to modify the Swagger Document - this time with request context
                foreach (var filter in _options.PreSerializeFilters)
                {
                    filter(swagger, httpContext.Request);
                }

                if (Path.GetExtension(httpContext.Request.Path.Value) == ".yaml")
                {
                    await RespondWithSwaggerYaml(httpContext.Response, swagger);
                }
                else
                {
                    await RespondWithSwaggerJson(httpContext.Response, swagger);
                }
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
            if (!_requestMatcher.TryMatch(request.Path, routeValues) || !routeValues.ContainsKey("documentName")) return false;

            documentName = routeValues["documentName"].ToString();
            return true;
        }

        private void RespondWithNotFound(HttpResponse response)
        {
            response.StatusCode = 404;
        }

        private async Task RespondWithSwaggerJson(HttpResponse response, OpenApiDocument swagger)
        {
            response.StatusCode = 200;
            response.ContentType = "application/json;charset=utf-8";

            using (var textWriter = new StringWriter(CultureInfo.InvariantCulture))
            {
                var jsonWriter = new OpenApiJsonWriter(textWriter);
                if (_options.SerializeAsV2) swagger.SerializeAsV2(jsonWriter); else swagger.SerializeAsV3(jsonWriter);

                await response.WriteAsync(textWriter.ToString(), new UTF8Encoding(false));
            }
        }

        private async Task RespondWithSwaggerYaml(HttpResponse response, OpenApiDocument swagger)
        {
            response.StatusCode = 200;
            response.ContentType = "text/yaml;charset=utf-8";

            using (var textWriter = new StringWriter(CultureInfo.InvariantCulture))
            {
                var yamlWriter = new OpenApiYamlWriter(textWriter);
                if (_options.SerializeAsV2) swagger.SerializeAsV2(yamlWriter); else swagger.SerializeAsV3(yamlWriter);

                await response.WriteAsync(textWriter.ToString(), new UTF8Encoding(false));
            }
        }
    }
}
