using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
#if !NETSTANDARD
using Microsoft.AspNetCore.Routing.Patterns;
#endif
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Writers;

namespace Swashbuckle.AspNetCore.Swagger
{
    internal sealed class SwaggerMiddleware
    {
        private static readonly Encoding UTF8WithoutBom = new UTF8Encoding(false);

        private readonly RequestDelegate _next;
        private readonly SwaggerOptions _options;
        private readonly TemplateMatcher _requestMatcher;
#if !NETSTANDARD
        private readonly TemplateBinder _templateBinder;
#endif

        public SwaggerMiddleware(
            RequestDelegate next,
            SwaggerOptions options)
        {
            _next = next;
            _options = options ?? new SwaggerOptions();
            _requestMatcher = new TemplateMatcher(TemplateParser.Parse(_options.RouteTemplate), []);
        }

#if !NETSTANDARD
        [ActivatorUtilitiesConstructor]
        public SwaggerMiddleware(
            RequestDelegate next,
            SwaggerOptions options,
            TemplateBinderFactory templateBinderFactory) : this(next, options)
        {
            _templateBinder = templateBinderFactory.Create(RoutePatternFactory.Parse(_options.RouteTemplate));
        }
#endif

        public async Task Invoke(HttpContext httpContext, ISwaggerProvider swaggerProvider)
        {
            if (!RequestingSwaggerDocument(httpContext.Request, out string documentName, out string extension))
            {
                await _next(httpContext);
                return;
            }

            try
            {
                var basePath = httpContext.Request.PathBase.HasValue
                    ? httpContext.Request.PathBase.Value
                    : null;

                OpenApiDocument swagger;
                var asyncSwaggerProvider = httpContext.RequestServices.GetService<IAsyncSwaggerProvider>();

                if (asyncSwaggerProvider is not null)
                {
                    swagger = await asyncSwaggerProvider.GetSwaggerAsync(
                        documentName: documentName,
                        host: null,
                        basePath: basePath);
                }
                else
                {
                    swagger = swaggerProvider.GetSwagger(
                        documentName: documentName,
                        host: null,
                        basePath: basePath);
                }

                // One last opportunity to modify the Swagger Document - this time with request context
                foreach (var filter in _options.PreSerializeFilters)
                {
                    filter(swagger, httpContext.Request);
                }

                if (extension is ".yaml" or ".yml")
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

        private bool RequestingSwaggerDocument(HttpRequest request, out string documentName, out string extension)
        {
            documentName = null;
            extension = null;

            if (!HttpMethods.IsGet(request.Method))
            {
                return false;
            }

            var routeValues = new RouteValueDictionary();
            if (_requestMatcher.TryMatch(request.Path, routeValues))
            {
#if !NETSTANDARD
                if (_templateBinder != null && !_templateBinder.TryProcessConstraints(request.HttpContext, routeValues, out _, out _))
                {
                    return false;
                }
#endif
                if (routeValues.TryGetValue("documentName", out var documentNameObject) && documentNameObject is string documentNameString)
                {
                    documentName = documentNameString;
                    if (routeValues.TryGetValue("extension", out var extensionObject))
                    {
                        extension = $".{extensionObject}";
                    }
                    else
                    {
                        extension = Path.GetExtension(request.Path.Value);
                    }
                    return true;
                }
            }

            return false;
        }

        private static void RespondWithNotFound(HttpResponse response)
        {
            response.StatusCode = 404;
        }

        private async Task RespondWithSwaggerJson(HttpResponse response, OpenApiDocument swagger)
        {
            string json;

            using (var textWriter = new StringWriter(CultureInfo.InvariantCulture))
            {
                var openApiWriter = new OpenApiJsonWriter(textWriter);

                WriteOpenApiDocumentContent(response, swagger, openApiWriter);

                json = textWriter.ToString();
            }

            response.StatusCode = 200;
            response.ContentType = "application/json;charset=utf-8";

            await response.WriteAsync(json, UTF8WithoutBom);
        }

        private async Task RespondWithSwaggerYaml(HttpResponse response, OpenApiDocument swagger)
        {
            string yaml;

            using (var textWriter = new StringWriter(CultureInfo.InvariantCulture))
            {
                var openApiWriter = new OpenApiYamlWriter(textWriter);

                WriteOpenApiDocumentContent(response, swagger, openApiWriter);

                yaml = textWriter.ToString();
            }

            response.StatusCode = 200;
            response.ContentType = "text/yaml;charset=utf-8";

            await response.WriteAsync(yaml, UTF8WithoutBom);
        }

        private void WriteOpenApiDocumentContent(
            HttpResponse response,
            OpenApiDocument swagger,
            OpenApiWriterBase openApiWriter)
        {
            if (_options.CustomDocumentSerializer != null)
            {
                _options.CustomDocumentSerializer.SerializeDocument(swagger, openApiWriter, _options.OpenApiVersion);
            }
            else
            {
                switch (_options.OpenApiVersion)
                {
                    case Microsoft.OpenApi.OpenApiSpecVersion.OpenApi2_0:
                        swagger.SerializeAsV2(openApiWriter);
                        break;

#if NET10_0_OR_GREATER
                    case Microsoft.OpenApi.OpenApiSpecVersion.OpenApi3_1:
                        swagger.SerializeAsV31(openApiWriter);
                        break;
#endif

                    case Microsoft.OpenApi.OpenApiSpecVersion.OpenApi3_0:
                    default:
                        swagger.SerializeAsV3(openApiWriter);
                        break;
                }
            }
        }
    }
}
