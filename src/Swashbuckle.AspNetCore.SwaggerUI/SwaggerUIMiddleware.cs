using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;

namespace Swashbuckle.AspNetCore.SwaggerUI;

internal sealed partial class SwaggerUIMiddleware
{
    private static readonly string SwaggerUIVersion = GetSwaggerUIVersion();

    private readonly RequestDelegate _next;
    private readonly SwaggerUIOptions _options;
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly EmbeddedResourceProvider _resourceProvider;

    public SwaggerUIMiddleware(RequestDelegate next, SwaggerUIOptions options)
    {
        _next = next;
        _options = options ?? new();

        if (options.JsonSerializerOptions != null)
        {
            _jsonSerializerOptions = options.JsonSerializerOptions;
        }

        var pathPrefix = options.RoutePrefix.StartsWith('/') ? options.RoutePrefix : $"/{options.RoutePrefix}";
        _resourceProvider = new(
            typeof(SwaggerUIMiddleware).Assembly,
            "Swashbuckle.AspNetCore.SwaggerUI.node_modules.swagger_ui_dist",
            pathPrefix,
            _options.CacheLifetime);
    }

    public async Task Invoke(HttpContext httpContext)
    {
        if (HttpMethods.IsGet(httpContext.Request.Method))
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

            var match = Regex.Match(path, $"^/{Regex.Escape(_options.RoutePrefix)}/?(index.(html|js))$", RegexOptions.IgnoreCase);

            if (match.Success)
            {
                await RespondWithFile(httpContext.Response, match.Groups[1].Value, httpContext.RequestAborted);
                return;
            }

            var pattern = $"^/?{Regex.Escape(_options.RoutePrefix)}/{_options.SwaggerDocumentUrlsPath}/?$";
            if (Regex.IsMatch(path, pattern, RegexOptions.IgnoreCase))
            {
                await RespondWithDocumentUrls(httpContext.Response);
                return;
            }
        }

        if (!await _resourceProvider.TryRespondWithFileAsync(httpContext))
        {
            await _next(httpContext);
        }
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

        headers.ETag = new($"\"{etag}\"", isWeak: true);
    }

    private static void RespondWithRedirect(HttpResponse response, string location)
    {
        response.StatusCode = StatusCodes.Status301MovedPermanently;
        response.Headers.Location = location;
    }

    private async Task RespondWithFile(
        HttpResponse response,
        string fileName,
        CancellationToken cancellationToken)
    {
        response.StatusCode = StatusCodes.Status200OK;

        Stream stream;

        if (fileName == "index.js")
        {
            response.ContentType = "application/javascript;charset=utf-8";
            stream = ResourceHelper.GetEmbeddedResource(fileName);
        }
        else
        {
            response.ContentType = "text/html;charset=utf-8";
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

            var content = new StringBuilder(template);

            foreach (var entry in GetIndexArguments())
            {
                content.Replace(entry.Key, entry.Value);
            }

            var text = content.ToString();
            var etag = HashText(text);

            SetHeaders(response, _options, etag);

            await response.WriteAsync(text, Encoding.UTF8, cancellationToken);
        }

        static string HashText(string text)
        {
            var buffer = Encoding.UTF8.GetBytes(text);
            var hash = SHA1.HashData(buffer);

            return Convert.ToBase64String(hash);
        }
    }

    [UnconditionalSuppressMessage(
        "AOT",
        "IL2026:RequiresUnreferencedCode",
        Justification = "Method is only called if the user provides their own custom JsonSerializerOptions.")]
    [UnconditionalSuppressMessage(
        "AOT",
        "IL3050:RequiresDynamicCode",
        Justification = "Method is only called if the user provides their own custom JsonSerializerOptions.")]
    private async Task RespondWithDocumentUrls(HttpResponse response)
    {
        response.StatusCode = 200;

        response.ContentType = "application/javascript;charset=utf-8";
        string json = "[]";

        if (_jsonSerializerOptions is null)
        {
            var l = new List<UrlDescriptor>(_options.ConfigObject.Urls);
            json = JsonSerializer.Serialize(l, SwaggerUIOptionsJsonContext.Default.ListUrlDescriptor);
        }

        json ??= JsonSerializer.Serialize(_options.ConfigObject, _jsonSerializerOptions);

        await response.WriteAsync(json, Encoding.UTF8);
    }

    [UnconditionalSuppressMessage(
        "AOT",
        "IL2026:RequiresUnreferencedCode",
        Justification = "Method is only called if the user provides their own custom JsonSerializerOptions.")]
    [UnconditionalSuppressMessage(
        "AOT",
        "IL3050:RequiresDynamicCode",
        Justification = "Method is only called if the user provides their own custom JsonSerializerOptions.")]
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
            { "%(DocumentTitle)", _options.DocumentTitle },
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
