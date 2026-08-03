using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;

namespace Swashbuckle.AspNetCore.ReDoc;

internal sealed partial class ReDocMiddleware
{
    private static readonly HashSet<string> AllowedHttpMethods = new(StringComparer.OrdinalIgnoreCase) { HttpMethods.Get, HttpMethods.Head };
    private static readonly string ReDocVersion = GetReDocVersion();

    private readonly RequestDelegate _next;
    private readonly ReDocOptions _options;
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly EmbeddedResourceProvider _resourceProvider;

    public ReDocMiddleware(RequestDelegate next, ReDocOptions options)
    {
        _next = next;
        _options = options ?? new ReDocOptions();

        if (options.JsonSerializerOptions != null)
        {
            _jsonSerializerOptions = options.JsonSerializerOptions;
        }

        var pathPrefix = options.RoutePrefix.StartsWith('/') ? options.RoutePrefix : $"/{options.RoutePrefix}";
        _resourceProvider = new(
            typeof(ReDocMiddleware).Assembly,
            "Swashbuckle.AspNetCore.ReDoc.node_modules.redoc.bundles",
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

            var match = Regex.Match(path, $@"^/{Regex.Escape(_options.RoutePrefix)}/?(index\.(html|css|js))$", RegexOptions.IgnoreCase);

            if (match.Success)
            {
                await RespondWithFile(httpContext, match.Groups[1].Value);
                return;
            }

            if (await _resourceProvider.TryRespondWithFileAsync(httpContext))
            {
                return;
            }
        }

        await _next(httpContext);
    }

    private static string GetReDocVersion()
        => typeof(ReDocMiddleware).Assembly
               .GetCustomAttributes<AssemblyMetadataAttribute>()
               .Where((p) => p.Key is "ReDocVersion")
               .Select((p) => p.Value)
               .DefaultIfEmpty(string.Empty)
               .FirstOrDefault();

    private static void SetHeaders(HttpResponse response, ReDocOptions options, string etag)
    {
        var headers = response.GetTypedHeaders();
        headers.Append("x-redoc-version", ReDocVersion);

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

        response.StatusCode = StatusCodes.Status200OK;

        Stream stream;

        // The route is matched case-insensitively, so the file must be selected the same way,
        // otherwise a request for "INDEX.JS" is answered with the HTML document instead. The
        // canonical name is used to look the resource up, as manifest names are case-sensitive.
        if (string.Equals(fileName, "index.css", StringComparison.OrdinalIgnoreCase))
        {
            response.ContentType = "text/css";
            stream = ResourceHelper.GetEmbeddedResource("index.css");
        }
        else if (string.Equals(fileName, "index.js", StringComparison.OrdinalIgnoreCase))
        {
            response.ContentType = "application/javascript;charset=utf-8";
            stream = ResourceHelper.GetEmbeddedResource("index.js");
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
                return;
            }

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

        static string GetETag(string text)
        {
            var buffer = Encoding.UTF8.GetBytes(text);
            var hash = SHA1.HashData(buffer);

            return $"\"{Convert.ToBase64String(hash)}\"";
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
    private Dictionary<string, string> GetIndexArguments()
    {
        string configObject = null;
        string specUrl = null;

        if (_jsonSerializerOptions is null)
        {
            configObject = JsonSerializer.Serialize(_options.ConfigObject, ReDocOptionsJsonContext.Default.ConfigObject);
            specUrl = JsonSerializer.Serialize(_options.SpecUrl ?? string.Empty, ReDocOptionsJsonContext.Default.String);
        }

        configObject ??= JsonSerializer.Serialize(_options.ConfigObject, _jsonSerializerOptions);
        specUrl ??= JsonSerializer.Serialize(_options.SpecUrl ?? string.Empty, _jsonSerializerOptions);

        return new Dictionary<string, string>()
        {
            { "%(DocumentTitle)", System.Net.WebUtility.HtmlEncode(_options.DocumentTitle) },
            { "%(HeadContent)", _options.HeadContent },
            { "%(SpecUrl)", specUrl },
            { "%(ConfigObject)", configObject },
        };
    }
}
