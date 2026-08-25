using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;

namespace Swashbuckle.AspNetCore.SwaggerUI;

internal sealed partial class SwaggerUIMiddleware
{
    private static readonly HashSet<string> AllowedHttpMethods = new(StringComparer.OrdinalIgnoreCase) { HttpMethods.Get, HttpMethods.Head };
    private static readonly string SwaggerUIVersion = GetSwaggerUIVersion();
    private static readonly JsonSerializerOptions DefaultJsonSerializerOptions = CreateDefaultJsonSerializerOptions();

    private readonly RequestDelegate _next;
    private readonly SwaggerUIOptions _options;
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly EmbeddedResourceProvider _resourceProvider;

    public SwaggerUIMiddleware(RequestDelegate next, SwaggerUIOptions options)
    {
        _next = next;
        _options = options ?? new();

        _jsonSerializerOptions = _options.JsonSerializerOptions ?? DefaultJsonSerializerOptions;

        var pathPrefix = options.RoutePrefix.StartsWith('/') ? options.RoutePrefix : $"/{options.RoutePrefix}";
        _resourceProvider = new(
            typeof(SwaggerUIMiddleware).Assembly,
            "Swashbuckle.AspNetCore.SwaggerUI.node_modules.swagger_ui_dist",
            pathPrefix,
            _options.CacheLifetime);
    }

    public async Task Invoke(HttpContext httpContext)
    {
        if (AllowedHttpMethods.Contains(httpContext.Request.Method))
        {
            var path = httpContext.Request.Path.Value;

            // If the RoutePrefix is requested (with or without trailing slash), redirect to index URL
            if (Regex.IsMatch(path, $"^/?{Regex.Escape(_options.RoutePrefix)}/?$", RegexOptions.IgnoreCase))
            {
                // Use relative redirect to support proxy environments
                var relativeIndexUrl =
                    string.IsNullOrEmpty(path) || path.EndsWith('/')
                    ? "index.html"
                    : $"{path.Split('/').Last()}/index.html";

                RespondWithRedirect(httpContext.Response, relativeIndexUrl);
                return;
            }

            var match = Regex.Match(path, $@"^/?{Regex.Escape(_options.RoutePrefix)}/?(index\.(html|js))$", RegexOptions.IgnoreCase);

            if (match.Success)
            {
                await RespondWithFile(httpContext, match.Groups[1].Value);
                return;
            }

            if (_options.ExposeSwaggerDocumentUrlsRoute)
            {
                var pattern = $"^/?{Regex.Escape(_options.RoutePrefix)}/{Regex.Escape(_options.SwaggerDocumentUrlsPath)}/?$";
                if (Regex.IsMatch(path, pattern, RegexOptions.IgnoreCase))
                {
                    await RespondWithDocumentUrls(httpContext);
                    return;
                }
            }

            if (await _resourceProvider.TryRespondWithFileAsync(httpContext))
            {
                return;
            }
        }

        await _next(httpContext);
    }

    [UnconditionalSuppressMessage(
        "AOT",
        "IL2026:RequiresUnreferencedCode",
        Justification = "The reflection-based resolver is only used when dynamic code is supported (i.e. not native AoT) to serialize custom values in ConfigObject.AdditionalItems. See https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/3153.")]
    [UnconditionalSuppressMessage(
        "AOT",
        "IL3050:RequiresDynamicCode",
        Justification = "The reflection-based resolver is only used when dynamic code is supported (i.e. not native AoT) to serialize custom values in ConfigObject.AdditionalItems. See https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/3153.")]
    private static JsonSerializerOptions CreateDefaultJsonSerializerOptions()
    {
        if (!RuntimeFeature.IsDynamicCodeSupported)
        {
            // Only source-generated metadata can be used with native AoT
            return null;
        }

        // Chain a reflection-based resolver after the source-generated one so that custom
        // values in ConfigObject.AdditionalItems (e.g. anonymous types) can be serialized.
        return new JsonSerializerOptions(SwaggerUIOptionsJsonContext.Default.Options)
        {
            TypeInfoResolver = JsonTypeInfoResolver.Combine(
                SwaggerUIOptionsJsonContext.Default,
                new DefaultJsonTypeInfoResolver()),
        };
    }

    private static string GetSwaggerUIVersion()
        => typeof(SwaggerUIMiddleware).Assembly
               .GetCustomAttributes<AssemblyMetadataAttribute>()
               .Where((p) => p.Key is "SwaggerUIVersion")
               .Select((p) => p.Value)
               .DefaultIfEmpty(string.Empty)
               .FirstOrDefault();

    private static void SetHeaders(HttpResponse response, SwaggerUIOptions options, string etag)
    {
        var headers = response.GetTypedHeaders();
        headers.Append("x-swagger-ui-version", SwaggerUIVersion);

        if (options.CacheLifetime is { } maxAge)
        {
            headers.CacheControl = new()
            {
                MaxAge = maxAge,
                Private = true,
            };
        }
        else
        {
            headers.CacheControl = new()
            {
                NoCache = true,
                NoStore = true,
            };
        }

        headers.ETag = new(etag);
    }

    private static void RespondWithRedirect(HttpResponse response, string location)
    {
        response.StatusCode = StatusCodes.Status301MovedPermanently;
        response.Headers.Location = location;
    }

    [GeneratedRegex(@"%\([A-Za-z]+\)")]
    private static partial Regex IndexArgumentPattern();

    private async Task RespondWithFile(HttpContext context, string fileName)
    {
        var cancellationToken = context.RequestAborted;
        var response = context.Response;

        string contentType;
        Stream stream;

        // The route is matched case-insensitively, so the file must be selected the same way,
        // otherwise a request for "INDEX.JS" is answered with the HTML document instead. The canonical
        // name is used to look the resource up, as manifest resource names are case-sensitive.
        if (string.Equals(fileName, "index.js", StringComparison.OrdinalIgnoreCase))
        {
            contentType = "application/javascript;charset=utf-8";
            stream = ResourceHelper.GetEmbeddedResource("index.js");
        }
        else
        {
            contentType = "text/html;charset=utf-8";
            stream = _options.IndexStream();
        }

        using (stream)
        {
            // Inject arguments before writing to response
            string template;

            using (var reader = new StreamReader(stream))
            {
                template = await reader.ReadToEndAsync(cancellationToken);
            }

            var arguments = GetIndexArguments();

            // Single pass over the original template: replacement values are never re-scanned for
            // further placeholder matches, so a value that happens to look like another placeholder
            // token cannot be substituted a second time.
            var text = IndexArgumentPattern().Replace(
                template,
                (match) => arguments.TryGetValue(match.Value, out var value) ? value : match.Value);

            var etag = GetETag(text);

            var ifNoneMatch = context.Request.Headers.IfNoneMatch;

            if (ifNoneMatch == etag)
            {
                response.StatusCode = StatusCodes.Status304NotModified;
            }
            else
            {
                response.ContentType = contentType;
                response.StatusCode = StatusCodes.Status200OK;

                SetHeaders(response, _options, etag);

                if (HttpMethods.IsGet(context.Request.Method))
                {
                    await response.WriteAsync(text, Encoding.UTF8, cancellationToken);
                }
                else if (HttpMethods.IsHead(context.Request.Method))
                {
                    // HEAD response must have an empty body, but have correct Content-Length header
                    response.ContentLength = Encoding.UTF8.GetByteCount(text);
                }
            }
        }
    }

    [UnconditionalSuppressMessage(
        "AOT",
        "IL2026:RequiresUnreferencedCode",
        Justification = "Reflection-based serialization is only used if the user provides their own custom JsonSerializerOptions or when dynamic code is supported.")]
    [UnconditionalSuppressMessage(
        "AOT",
        "IL3050:RequiresDynamicCode",
        Justification = "Reflection-based serialization is only used if the user provides their own custom JsonSerializerOptions or when dynamic code is supported.")]
    private async Task RespondWithDocumentUrls(HttpContext context)
    {
        var response = context.Response;
        var urls = _options.ConfigObject.Urls ?? [];

        string json =
            _jsonSerializerOptions is { } options ?
            JsonSerializer.Serialize(urls, options) :
            JsonSerializer.Serialize([.. urls], SwaggerUIOptionsJsonContext.Default.ListUrlDescriptor);

        var etag = GetETag(json);
        var ifNoneMatch = context.Request.Headers.IfNoneMatch;

        if (ifNoneMatch == etag)
        {
            response.StatusCode = StatusCodes.Status304NotModified;
        }
        else
        {
            response.StatusCode = StatusCodes.Status200OK;
            response.ContentType = "application/javascript;charset=utf-8";

            SetHeaders(response, _options, etag);

            await response.WriteAsync(json, Encoding.UTF8, context.RequestAborted);
        }
    }

    private static string GetETag(string text)
    {
        var buffer = Encoding.UTF8.GetBytes(text);
        var hash = SHA1.HashData(buffer);

        return $"\"{Convert.ToBase64String(hash)}\"";
    }

    [UnconditionalSuppressMessage(
        "AOT",
        "IL2026:RequiresUnreferencedCode",
        Justification = "Reflection-based serialization is only used if the user provides their own custom JsonSerializerOptions or when dynamic code is supported.")]
    [UnconditionalSuppressMessage(
        "AOT",
        "IL3050:RequiresDynamicCode",
        Justification = "Reflection-based serialization is only used if the user provides their own custom JsonSerializerOptions or when dynamic code is supported.")]
    private Dictionary<string, string> GetIndexArguments()
    {
        string configObject = null;
        string oauthConfigObject = null;
        string interceptors = null;

        if (_jsonSerializerOptions is null)
        {
            configObject = JsonSerializer.Serialize(_options.ConfigObject, SwaggerUIOptionsJsonContext.Default.ConfigObject);
            oauthConfigObject = JsonSerializer.Serialize(_options.OAuthConfigObject, SwaggerUIOptionsJsonContext.Default.OAuthConfigObject);
            interceptors = JsonSerializer.Serialize(_options.Interceptors, SwaggerUIOptionsJsonContext.Default.InterceptorFunctions);
        }

        configObject ??= JsonSerializer.Serialize(_options.ConfigObject, _jsonSerializerOptions);
        oauthConfigObject ??= JsonSerializer.Serialize(_options.OAuthConfigObject, _jsonSerializerOptions);
        interceptors ??= JsonSerializer.Serialize(_options.Interceptors, _jsonSerializerOptions);

        return new Dictionary<string, string>()
        {
            { "%(DocumentTitle)", System.Net.WebUtility.HtmlEncode(_options.DocumentTitle) },
            { "%(HeadContent)", _options.HeadContent },
            { "%(StylesPath)", _options.StylesPath },
            { "%(ScriptBundlePath)", _options.ScriptBundlePath },
            { "%(ScriptPresetsPath)", _options.ScriptPresetsPath },
            { "%(ConfigObject)", configObject },
            { "%(OAuthConfigObject)", oauthConfigObject },
            { "%(Interceptors)", interceptors },
        };
    }
}
