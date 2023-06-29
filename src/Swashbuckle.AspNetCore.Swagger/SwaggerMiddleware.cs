using System;
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
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Writers;

namespace Swashbuckle.AspNetCore.Swagger
{
    public class SwaggerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly SwaggerOptions _options;
        private readonly TemplateMatcher _requestMatcher;
        private readonly ISwaggerDocumentSerializer _swaggerDocumentSerializer;
#if !NETSTANDARD
        private readonly TemplateBinder _templateBinder;
#endif

        public SwaggerMiddleware(
            RequestDelegate next,
            SwaggerOptions options,
            IServiceProvider serviceProvider)
        {
            _next = next;
            _options = options ?? new SwaggerOptions();
            _requestMatcher = new TemplateMatcher(TemplateParser.Parse(_options.RouteTemplate), new RouteValueDictionary());

            // Use IServiceProvider to retrieve the ISwaggerDocumentSerializer, because it is an optional service
            _swaggerDocumentSerializer = serviceProvider.GetService(typeof(ISwaggerDocumentSerializer)) as ISwaggerDocumentSerializer;
        }

#if !NETSTANDARD
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

                var swagger = swaggerProvider switch
                {
                    IAsyncSwaggerProvider asyncSwaggerProvider => await asyncSwaggerProvider.GetSwaggerAsync(
                        documentName: documentName,
                        host: null,
                        basePath: basePath),
                    _ => swaggerProvider.GetSwagger(
                        documentName: documentName,
                        host: null,
                        basePath: basePath)
                };

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
            if (request.Method != "GET") return false;

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
                if (_options.SerializeAsV2)
                {
                    if (_swaggerDocumentSerializer != null)
                        _swaggerDocumentSerializer.SerializeDocument(swagger, jsonWriter, Microsoft.OpenApi.OpenApiSpecVersion.OpenApi2_0);
                    else
                        swagger.SerializeAsV2(jsonWriter);
                }
                else
                {
                    if (_swaggerDocumentSerializer != null)
                        _swaggerDocumentSerializer.SerializeDocument(swagger, jsonWriter, Microsoft.OpenApi.OpenApiSpecVersion.OpenApi3_0);
                    else
                        swagger.SerializeAsV3(jsonWriter);
                }

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
                if (_options.SerializeAsV2)
                {
                    if (_swaggerDocumentSerializer != null)
                        _swaggerDocumentSerializer.SerializeDocument(swagger, yamlWriter, Microsoft.OpenApi.OpenApiSpecVersion.OpenApi2_0);
                    else
                        swagger.SerializeAsV2(yamlWriter);
                }
                else
                {
                    if (_swaggerDocumentSerializer != null)
                        _swaggerDocumentSerializer.SerializeDocument(swagger, yamlWriter, Microsoft.OpenApi.OpenApiSpecVersion.OpenApi3_0);
                    else
                        swagger.SerializeAsV3(yamlWriter);
                }

                await response.WriteAsync(textWriter.ToString(), new UTF8Encoding(false));
            }
        }
    }
}
