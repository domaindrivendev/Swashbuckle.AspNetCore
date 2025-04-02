using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
#if NET
using Microsoft.AspNetCore.Routing.Patterns;
#endif
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Writers;

namespace Swashbuckle.AspNetCore.Swagger;

internal sealed class SwaggerMiddleware
{
    private static readonly Encoding UTF8WithoutBom = new UTF8Encoding(false);

    private readonly RequestDelegate _next;
    private readonly SwaggerOptions _options;
    private readonly TemplateMatcher _requestMatcher;
#if NET
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

#if NET
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
            httpContext.Response.StatusCode = 404;
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
#if NET
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

    private async Task RespondWithSwaggerJson(HttpResponse response, OpenApiDocument swagger)
    {
        string json;

        using (var textWriter = new StringWriter(CultureInfo.InvariantCulture))
        {
            var openApiWriter = new OpenApiJsonWriter(textWriter);

            SerializeDocument(swagger, openApiWriter);

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

            SerializeDocument(swagger, openApiWriter);

            yaml = textWriter.ToString();
        }

        response.StatusCode = 200;
        response.ContentType = "text/yaml;charset=utf-8";

        await response.WriteAsync(yaml, UTF8WithoutBom);
    }

    private void SerializeDocument(
        OpenApiDocument document,
        IOpenApiWriter writer)
    {
        if (_options.CustomDocumentSerializer != null)
        {
            _options.CustomDocumentSerializer.SerializeDocument(document, writer, _options.OpenApiVersion);
        }
        else
        {
#if false
            // TODO Use SerializeAs() when available
            document.SerializeAs(writer, _options.OpenApiVersion);
#endif

            switch (_options.OpenApiVersion)
            {
                case Microsoft.OpenApi.OpenApiSpecVersion.OpenApi2_0:
                    document.SerializeAsV2(writer);
                    break;

                case Microsoft.OpenApi.OpenApiSpecVersion.OpenApi3_1:
                    document.SerializeAsV31(writer);
                    break;

                case Microsoft.OpenApi.OpenApiSpecVersion.OpenApi3_0:
                default:
                    document.SerializeAsV3(writer);
                    break;
            }
        }
    }
}
