using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.Extensions.Primitives;
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
                var swagger = swaggerProvider.GetSwagger(
                    documentName: documentName,
                    host: GetHostOrNullFromRequest(httpContext.Request),
                    basePath: GetBasePathOrNullFromRequest(httpContext.Request));

                // One last opportunity to modify the Swagger Document - this time with request context
                foreach (var filter in _options.PreSerializeFilters)
                {
                    filter(swagger, httpContext.Request);
                }

                if (Path.GetExtension(httpContext.Request.Path.Value) == "yaml")
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

        private string GetHostOrNullFromRequest(HttpRequest request)
        {
            // Honor common reverse proxy headers if present ...

            UriBuilder hostBuilder = null;

            if (request.Headers.TryGetValue("X-Forwarded-For", out StringValues forwardedFor))
                hostBuilder = new UriBuilder($"http://{forwardedFor[0]}");

            if (request.Headers.TryGetValue("X-Forwarded-Host", out StringValues forwardedHost))
                hostBuilder = new UriBuilder($"http://{forwardedHost[0]}");

            if (hostBuilder == null)
                return null;

            if (request.Headers.TryGetValue("X-Forwarded-Proto", out StringValues forwardedProto))
                hostBuilder.Scheme = forwardedProto[0];

            if (request.Headers.TryGetValue("X-Forwarded-Port", out StringValues forwardedPort))
                hostBuilder.Port = int.Parse(forwardedPort[0]);

            return hostBuilder.Uri.ToString().Trim('/');
        }

        private string GetBasePathOrNullFromRequest(HttpRequest request)
        {
            var pathBuilder = new StringBuilder();

            if (request.Headers.TryGetValue("X-Forwarded-Prefix", out StringValues forwardedPrefix))
                pathBuilder.Append(forwardedPrefix[0].TrimEnd('/'));

            if (request.PathBase.HasValue)
                pathBuilder.Append(request.PathBase.Value.TrimEnd('/'));

            return (pathBuilder.Length > 0)
                ? pathBuilder.ToString()
                : null;
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
