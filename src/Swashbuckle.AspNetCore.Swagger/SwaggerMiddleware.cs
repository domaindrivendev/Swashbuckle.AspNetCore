using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;

namespace Swashbuckle.AspNetCore.Swagger;

internal sealed class SwaggerMiddleware
{
    private static readonly Encoding UTF8WithoutBom = new UTF8Encoding(false);
    private static readonly HashSet<string> AllowedHttpMethods = new(StringComparer.OrdinalIgnoreCase) { HttpMethods.Get, HttpMethods.Head };

    private static readonly HashSet<string> DefaultAllowedExtensions = new(StringComparer.OrdinalIgnoreCase) { "json", "yaml", "yml" };

    private readonly RequestDelegate _next;
    private readonly SwaggerOptions _options;
    private readonly TemplateMatcher _requestMatcher;
    private readonly TemplateBinder _templateBinder;
    private readonly bool _applyDefaultExtensionConstraint;

    public SwaggerMiddleware(
        RequestDelegate next,
        SwaggerOptions options)
    {
        _next = next;
        _options = options ?? new SwaggerOptions();

        var routeTemplate = TemplateParser.Parse(_options.RouteTemplate);
        _requestMatcher = new TemplateMatcher(routeTemplate, []);

        // The default route template uses an unconstrained {extension} parameter so that applications
        // configured with CreateSlimBuilder()/AddRoutingCore(), which do not register the regex route
        // constraint, can still resolve a TemplateBinder for it (see #2951). To keep only the supported
        // extensions matching, apply the equivalent allow-list in code when the parameter is unconstrained.
        _applyDefaultExtensionConstraint = routeTemplate.Parameters.Any(
            static parameter => parameter.Name == "extension" && !parameter.InlineConstraints.Any());
    }

    [ActivatorUtilitiesConstructor]
    public SwaggerMiddleware(
        RequestDelegate next,
        SwaggerOptions options,
        TemplateBinderFactory templateBinderFactory) : this(next, options)
    {
        _templateBinder = templateBinderFactory.Create(RoutePatternFactory.Parse(_options.RouteTemplate));
    }

    public async Task Invoke(HttpContext httpContext, ISwaggerProvider swaggerProvider)
    {
        if (!RequestingSwaggerDocument(httpContext.Request, out string documentName, out string extension))
        {
            await _next(httpContext);
            return;
        }

        try
        {
            var basePath = GetBasePath(httpContext.Request);

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

            if (basePath is not null)
            {
                // The document embeds the request's path base, which is not necessarily fixed by the
                // application (see GetBasePath), so the response is not safe for a shared cache to
                // store and replay to a client whose request had a different path base.
                httpContext.Response.GetTypedHeaders().CacheControl = new() { Private = true };
            }

            var isHeadRequest = HttpMethods.IsHead(httpContext.Request.Method);

            if (extension is ".yaml" or ".yml")
            {
                await RespondWithSwaggerYaml(httpContext.Response, swagger, isHeadRequest);
            }
            else
            {
                await RespondWithSwaggerJson(httpContext.Response, swagger, isHeadRequest);
            }
        }
        catch (UnknownSwaggerDocument)
        {
            httpContext.Response.StatusCode = 404;
        }
    }

    /// <summary>
    /// Gets the base path to use for the document's server URL, or <see langword="null"/> if there is none.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The value becomes <c>servers[].url</c>, which consumers such as the Swagger UI resolve as a URI
    /// reference to determine where to send requests. <see cref="HttpRequest.PathBase"/> is only ever a
    /// path, but a value beginning <c>//</c> or <c>/\</c> is a network-path reference that a browser
    /// resolves against a different authority, so such a value is discarded rather than emitted.
    /// </para>
    /// <para>
    /// This matters because the path base is not necessarily fixed by the application: it is derived
    /// from the request when <c>X-Forwarded-Prefix</c> is honoured by the forwarded headers middleware.
    /// </para>
    /// </remarks>
    private static string GetBasePath(HttpRequest request)
    {
        if (!request.PathBase.HasValue)
        {
            return null;
        }

        var basePath = request.PathBase.Value;

        if (basePath.Length > 1 && basePath[0] is '/' && basePath[1] is '/' or '\\')
        {
            return null;
        }

        return basePath;
    }

    private bool RequestingSwaggerDocument(HttpRequest request, out string documentName, out string extension)
    {
        documentName = null;
        extension = null;

        if (!AllowedHttpMethods.Contains(request.Method))
        {
            return false;
        }

        var routeValues = new RouteValueDictionary();
        if (_requestMatcher.TryMatch(request.Path, routeValues))
        {
            if (_templateBinder != null && !_templateBinder.TryProcessConstraints(request.HttpContext, routeValues, out _, out _))
            {
                return false;
            }

            if (routeValues.TryGetValue("documentName", out var documentNameObject) && documentNameObject is string documentNameString)
            {
                if (routeValues.TryGetValue("extension", out var extensionObject))
                {
                    if (_applyDefaultExtensionConstraint &&
                        (extensionObject is not string extensionString || !DefaultAllowedExtensions.Contains(extensionString)))
                    {
                        return false;
                    }

                    extension = $".{extensionObject}";
                }
                else
                {
                    extension = Path.GetExtension(request.Path.Value);
                }

                documentName = documentNameString;
                return true;
            }
        }

        return false;
    }

    private async Task RespondWithSwaggerJson(HttpResponse response, OpenApiDocument swagger, bool isHeadRequest)
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

        if (isHeadRequest)
        {
            // HEAD response must have an empty body, but have correct Content-Length header
            response.ContentLength = UTF8WithoutBom.GetByteCount(json);
        }
        else
        {
            await response.WriteAsync(json, UTF8WithoutBom);
        }
    }

    private async Task RespondWithSwaggerYaml(HttpResponse response, OpenApiDocument swagger, bool isHeadRequest)
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

        if (isHeadRequest)
        {
            // HEAD response must have an empty body, but have correct Content-Length header
            response.ContentLength = UTF8WithoutBom.GetByteCount(yaml);
        }
        else
        {
            await response.WriteAsync(yaml, UTF8WithoutBom);
        }
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
            document.SerializeAs(_options.OpenApiVersion, writer);
        }
    }
}
